from pydantic import Field, ValidationError

from hyperedge.sdk.models.types import *
from hyperedge.sdk.models.base import _BaseModel
from hyperedge.sdk.utils import to_underscore


class BaseData(_BaseModel):
    _abstract = True

    _classes = {}
    _registry = {}
    _lists = {}

    id: str = Field(...)

    def __init__(self, **data):
        super().__init__(**data)

    def __hash__(self):
        return hash(self.id)

    def __eq__(self, other):
        return self.id == other.id

    @staticmethod
    def dataclasses():
        return BaseData._classes.values()

    @classmethod
    def instances(cls, predicate=None):
        return list(cls.iter_instances(predicate=predicate))

    @classmethod
    def iter_instances(cls, predicate=None):
        for inst in BaseData._lists[cls.__name__]:
            if predicate and not predicate(inst):
                continue
            yield inst

    @classmethod
    def process_data_refs(cls, fname, ftype, fval):
        t_origin = typing.get_origin(ftype)
        if t_origin is DataRef:
            t_args = typing.get_args(ftype)
            if not isinstance(fval, t_args[0]):
                raise Exception(
                    f"Can't create instance of '{cls.__name__}': field '{fname}' should be of type {fdef.outer_type_}")
            return DataRef[t_args[0]](ref=fval.id, dt=t_args[0])
        elif t_origin is list:
            t_args = typing.get_args(ftype)
            return [cls.process_data_refs(fname, t_args[0], el) for el in fval]
        else:
            return fval

    @classmethod
    def define(cls, **kwargs):
        if 'id' not in kwargs and 'Name' in kwargs:
            kwargs['id'] = to_underscore(kwargs['Name'])
        #
        for fname, fdef in cls.__fields__.items():
            if fname not in kwargs:
                if not fdef.required:
                    continue
                raise Exception(f"Can't create instance of '{cls.__name__}': field '{fname}' is undefined")
            #
            fval = kwargs[fname]
            kwargs[fname] = cls.process_data_refs(fname, fdef.outer_type_, fval)
        #
        inst = cls(**kwargs)
        if cls.__name__ not in BaseData._registry:
            BaseData._classes[cls.__name__] = cls
            BaseData._registry[cls.__name__] = {}
            BaseData._lists[cls.__name__] = []
        if inst.id in BaseData._registry[cls.__name__]:
            raise Exception(f"Instance of '{cls.__name__}' with id='{inst.id}' is already defined")
        BaseData._registry[cls.__name__][inst.id] = inst
        BaseData._lists[cls.__name__].append(inst)
        return inst

    @staticmethod
    def get_cls_by_name(cls_name: str):
        return BaseData._classes.get(cls_name)

    @staticmethod
    def get_data_instance(cls_name: str, inst_id: str) -> 'BaseData':
        return BaseData._registry[cls_name][inst_id]

    @classmethod
    def to_dict(cls):
        d = super().to_dict()
        base = cls.base(BaseData)
        d['Base'] = base.__name__ if base else None
        return d


class UGCData(BaseData):
    pass


ReferencedType = typing.TypeVar('ReferencedType')


class DataRef(typing.Generic[ReferencedType]):

    def __init__(self, ref: str, dt: typing.Type):
        self._ref = ref
        self._dt = dt

    @property
    def ref_str(self):
        return f"{self._dt.__name__}/{self._ref}"

    @classmethod
    def __get_validators__(cls):
        yield cls.validate_ref

    @classmethod
    def validate_ref(cls, v):
        if isinstance(v, str):
            return v
        if not isinstance(v, cls):
            raise ValidationError("Data reference should be DataRef")
        return v.ref_str

    def __repr__(self):
        return self.ref_str
