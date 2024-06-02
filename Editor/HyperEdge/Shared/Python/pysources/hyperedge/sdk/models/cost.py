import pydantic
import typing

from hyperedge.sdk.models.types import *
from hyperedge.sdk.models.models import DataModel, DataModelTemplate
from hyperedge.sdk.models.data import BaseData


class Cost(pydantic.BaseModel):
    assets: typing.List[typing.Tuple[DataModelTemplate, int]]
    items: typing.List[typing.Tuple[BaseData, int]]

    def __init__(self):
        super().__init__(assets=[], items=[])

    def add(self, item, amount: int = 1):
        if isinstance(item, BaseData):
            self.items.append((item, amount))
        elif isinstance(item, DataModelTemplate):
            self.assets.append((item, amount))
        else:
            assert False
        return self

    def to_dict(self):
        return dict(
            Erc721Costs=[],
            Erc1155Costs=[dict(
                ItemId=f'{type(item).__name__}/{item.id}',
                Amount=amount
            ) for item, amount in self.items]
        )

    @classmethod
    def from_dict(cls, data: dict):
        cost = cls()
        #
        for c_data in data['Erc721Costs']:
            item = DataModel.get_template(c_data['EntityName'], c_data['ItemId'])
            cost.add(item, c_data['Amount'])
        #
        for c_data in data['Erc1155Costs']:
            cls_name, inst_id = c_data['ItemId'].split('/')
            item = BaseData.get_data_instance(cls_name, inst_id)
            cost.add(item, c_data['Amount'])
        #
        return cost
