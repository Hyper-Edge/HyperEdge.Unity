import pydantic
import typing

from hyperedge.sdk.models.base import _BaseModel
from hyperedge.sdk.models.data import DataRef, BaseData
from hyperedge.sdk.models.models import DataModel
from hyperedge.sdk.models.types import optional_field

from hyperedge.sdk.utils import camelize_string


class MessageHubSystem(object):
    def __init__(self, name: str, standalone: bool = False):
        self._name = name
        self._standalone = standalone
        #
        self._MessageTypes = []

    def to_dict(self):
        return dict(
            Name=self._name,
            Standalone=self._standalone,
            MessageTypes=[el.to_dict() for el in self._MessageTypes]
        )
