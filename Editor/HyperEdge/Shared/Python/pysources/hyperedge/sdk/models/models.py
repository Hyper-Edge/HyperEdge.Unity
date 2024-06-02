from .base import _BaseModel
from .types import *
from .data import BaseData


class DataModelMeta(type(_BaseModel)):
    _registry = {}
    _data_classes = {}
    _templates = {}

    def __new__(mcs, cls_name, bases, namespace):
        new_cls = super().__new__(mcs, cls_name, bases, namespace)
        mcs._registry[cls_name] = new_cls
        mcs._data_classes[cls_name] = None
        mcs._templates[cls_name] = {}
        return new_cls


class DataModelTemplate(object):
    def __init__(self, cls, data):
        self._cls = cls
        self._data = data

    @property
    def entity_name(self):
        return self._cls.__name__

    @property
    def id(self):
        return self._data.id

    @classmethod
    def __get_validators__(cls):
        yield cls.validate

    @classmethod
    def validate(cls, v):
        return v


class DataModel(_BaseModel, metaclass=DataModelMeta):
    _abstract = True

    def __getattr__(cls, name):
        if name in DataModelMeta._templates[cls.__name__]:
            return DataModelMeta._templates[cls.__name__][name]
        else:
            raise AttributeError(f"{cls.__name__} object has no attribute {name}")

    @staticmethod
    def get_template(cls_name: str, inst_id: str) -> DataModelTemplate:
        cls = DataModelMeta._registry.get(cls_name)
        if cls is None:
            raise Exception(f"DataModel '{cls_name}' doesn't exist")
        #
        data_cls = cls.dataclass()
        di = BaseData.get_data_instance(data_cls.__name__, inst_id)
        if di:
            return cls.new_template(di)
        #
        return getattr(cls, inst_id)

    @classmethod
    def new_template(cls, data: BaseData) -> DataModelTemplate:
        if not isinstance(data, cls.dataclass()):
            raise TypeError()
        tmpl = DataModelTemplate(cls, data)
        DataModelMeta._templates[cls.__name__][data.id] = tmpl
        return tmpl

    @classmethod
    def to_dict(cls):
        d = super().to_dict()
        base = cls.base(DataModel)
        d['Base'] = base.__name__ if base else None
        return d

    @classmethod
    def dataclass(cls):
        data_cls = DataModelMeta._data_classes.get(cls.__name__)
        if data_cls:
            return data_cls
        data_cls = cls._get_dataclass()
        DataModelMeta._data_classes[cls.__name__] = data_cls
        return data_cls

    @classmethod
    def _get_dataclass(cls):
        dcs = []
        data_fld_cls = None
        for fname, fdef in cls.__fields__.items():
            t_origin = typing.get_origin(fdef.outer_type_)
            if not t_origin:
                continue
            if t_origin.__name__ != 'DataRef':
                continue
            t_args = typing.get_args(fdef.outer_type_)
            dcs.append(t_args[0])
            if fname == 'Data':
                data_fld_cls = t_args[0]
        if data_fld_cls:
            return data_fld_cls
        return dcs[0] if dcs else None


class Upgradeable(DataModel):
    _abstract = True
    Level: int


class UpgradeableWithExp(DataModel):
    _abstract = True
    Exp: UInt64 = 0
    Level: UInt32 = 0
