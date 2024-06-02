import typing
import os
import pathlib

from .base import _BaseModel
from .types import optional_field


class Job(_BaseModel):
    Name: str
    JobDataClass: typing.Type[_BaseModel]
    Code: str = optional_field('')

    def to_dict(self):
        return dict(
            Name=self.Name,
            JobDataClassName=self.JobDataClass.__name__,
            Code=self.Code or self.load_code())

    def _get_hcs_path(self):
        return pathlib.Path(os.environ['HE_APP_PATH']).joinpath('server_scripts', 'Jobs', f'{self.Name}.hcs')

    def load_code(self) -> str:
        h_path = self._get_hcs_path()
        with open(h_path, 'r') as cs_f:
            self.Code = cs_f.read()
        return self.Code

