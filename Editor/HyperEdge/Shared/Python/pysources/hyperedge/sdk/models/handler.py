import os
import pathlib
import typing

from .base import _BaseModel
from .types import optional_field


class Handler(_BaseModel):
    Name: str
    RequestClass: typing.Type[_BaseModel]
    ResponseClass: typing.Type[_BaseModel]
    Code: str = optional_field('')

    def to_dict(self):
        return dict(
            Name=self.Name,
            RequestClassName=self.RequestClass.__name__,
            ResponseClassName=self.ResponseClass.__name__,
            Code=self.Code or self.load_code()
        )

    def _get_hcs_path(self):
        return pathlib.Path(os.environ['HE_APP_PATH']).joinpath('server_scripts', 'Handlers', f'{self.Name}.hcs')

    def load_code(self) -> str:
        h_path = self._get_hcs_path()
        with open(h_path, 'r') as cs_f:
            self.Code = cs_f.read()
        return self.Code

    def save_code(self):
        h_path = self._get_hcs_path()
        #
        if not h_path.parent.exists():
            os.makedirs(str(h_path.parent), exist_ok=True)
        #
        with open(h_path, 'w') as hcs_f:
            hcs_f.write(self.Code)

