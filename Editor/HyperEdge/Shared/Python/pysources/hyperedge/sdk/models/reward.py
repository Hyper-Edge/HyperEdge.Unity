import pydantic

from hyperedge.sdk.models.types import *
from hyperedge.sdk.models.models import DataModel, DataModelTemplate
from hyperedge.sdk.models.data import BaseData


class Reward(pydantic.BaseModel):
    assets: typing.List[typing.Tuple[DataModelTemplate, int]]
    items: typing.List[typing.Tuple[BaseData, int]]
    name: typing.Optional[str]

    def __init__(self, name: str = ''):
        super().__init__(name=name, assets=[], items=[])

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
            Name=self.name,
            Erc721Rewards=[
                dict(
                    EntityName=item.entity_name,
                    ItemId=item.id,
                    Amount=amount
                ) for item, amount in self.assets
            ],
            Erc1155Rewards=[dict(
                ItemId=f'{type(item).__name__}/{item.id}',
                Amount=amount
            ) for item, amount in self.items]
        )

    @classmethod
    def from_dict(cls, data: dict):
        reward = cls(data.get('Name', ''))
        #
        for r_data in data['Erc721Rewards']:
            asset = DataModel.get_template(r_data['EntityName'], r_data['ItemId'])
            reward.add(asset, r_data['Amount'])
        #
        for r_data in data['Erc1155Rewards']:
            cls_name, inst_id = r_data['ItemId'].split('/')
            item = BaseData.get_data_instance(cls_name, inst_id)
            reward.add(item, r_data['Amount'])
        #
        return reward

