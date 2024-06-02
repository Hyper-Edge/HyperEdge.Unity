from pydantic import BaseModel
import six

from .types import get_cs_type, Ulid


class _BaseModelMeta(type(BaseModel)):
    _ATTRS = (
        '_abstract',
        '_tokenized',
        '_nft',
        '_dto',
        '_upgrade_material',
        '_upgrade_target',
        '_is_ladder',
        '_struct'
    )

    def __new__(mcs, cls_name, bases, namespace):
        new_cls = super().__new__(mcs, cls_name, bases, namespace)
        for key in mcs._ATTRS:
            if key.startswith('_'):
                setattr(new_cls, key, namespace.get(key, False))
        return new_cls


class _BaseModel(six.with_metaclass(_BaseModelMeta, BaseModel)):
    _abstract = True

    @classmethod
    def is_nft(cls):
        return cls._nft

    @classmethod
    def is_erc1155(cls):
        return cls._tokenized or\
               cls._upgrade_material

    @classmethod
    def is_erc721(cls):
        return cls._nft

    @classmethod
    def base(cls, b_cls):
        found_data_model = False
        for p_cls in reversed(cls.__mro__):
            if p_cls is b_cls:
                found_data_model = True
            #
            elif found_data_model and issubclass(p_cls, b_cls) and cls != p_cls:
                return p_cls
        return None

    @classmethod
    def to_dict(cls):
        flds = []
        for fname, fdef in cls.__fields__.items():
            if fname == 'id':
                continue
            flds.append({'Name': fname, 'Typename': get_cs_type(fdef.outer_type_)})
        return dict(
            Name=cls.__name__,
            Fields=flds
        )


class StructModel(_BaseModel):
    Id: Ulid
