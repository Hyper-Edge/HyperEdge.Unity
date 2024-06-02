import os
import pathlib
import typing

from .base import _BaseModel
from .types import Ulid, optional_field


class BaseEvent(_BaseModel):
    _abstract = True

    @classmethod
    def to_dict(cls):
        d = super().to_dict()
        base = cls.base(BaseEvent)
        d['Base'] = base.__name__ if base else None
        return d


class OnRegisterEvent(BaseEvent):
    UserId: Ulid


class OnLoginEvent(BaseEvent):
    UserId: Ulid


class EventHandler(_BaseModel):
    Name: str = optional_field('')
    EventClass: typing.Type[BaseEvent]
    Code: str = optional_field('')

    def to_dict(self):
        return dict(
            Name=self.Name,
            EventClassName=self.EventClass.__name__,
            Code=self.load_code())

    def _get_hcs_path(self):
        return pathlib.Path(os.environ['HE_APP_PATH']).joinpath('server_scripts', 'EventHandlers', f'{self.Name}.hcs')

    def load_code(self) -> str:
        h_path = self._get_hcs_path()
        with open(h_path, 'r') as cs_f:
            self.Code = cs_f.read()
        return self.Code

