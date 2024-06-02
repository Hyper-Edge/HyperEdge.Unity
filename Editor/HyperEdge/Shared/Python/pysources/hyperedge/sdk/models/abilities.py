import itertools
import pydantic
import ulid
import typing

try:
    import networkx as nx
except:
    pass

from hyperedge.sdk.models.base import _BaseModel
from hyperedge.sdk.models.models import DataModel
from hyperedge.sdk.models.types import optional_field
from hyperedge.sdk.utils import camelize_string


PortValueType = typing.TypeVar('PortValueType')


class InPortType(typing.Generic[PortValueType]):
    pass


class OutPortType(typing.Generic[PortValueType]):
    pass


class AbilityGraphException(Exception):
    pass


class PortBase(object):
    _port_id = itertools.count(0)

    def __init__(self, node: 'AbilityGraphNode', name: str, value_type: typing.Type):
        self._id = next(self._port_id)
        self._node = node
        self._name = name
        self._value_type = value_type

    @property
    def id(self) -> int:
        return self._id

    @property
    def name(self) -> str:
        return self._name

    @property
    def node(self) -> 'AbilityGraphNode':
        return self._node

    @property
    def is_input(self):
        return False

    @property
    def is_output(self):
        return False

    def connect(self, port: 'PortBase'):
        if port.is_input == port.is_output:
            raise AbilityGraphException()
        src_port = self if self.is_output else port
        dst_port = self if self.is_input else port
        return self._node.graph.connect_nodes(src_port, dst_port)


class InPort(PortBase):
    def __init__(self, node: 'AbilityGraphNode', name: str, value_type: typing.Type):
        super().__init__(node, name, value_type)

    @property
    def is_input(self):
        return True

    @property
    def is_output(self):
        return False


class OutPort(PortBase):
    def __init__(self, node: 'AbilityGraphNode', name: str, value_type: typing.Type):
        super().__init__(node, name, value_type)
        self._outputs = []

    @property
    def is_input(self):
        return False

    @property
    def is_output(self):
        return True

    def connect(self, in_port: InPort):
        pass


class AbilityNodeSpec(pydantic.BaseModel):
    pass


class AbilityNode(_BaseModel):
    Name: str
    Category: str
    Base: str
    Inputs: typing.Dict[str, typing.Type]
    Outputs: typing.Dict[str, typing.Type]
    Code: typing.Optional[str]

    _SpecClass: typing.Type[AbilityNodeSpec] = optional_field(None)

    @property
    def spec(self):
        if self._SpecClass:
            return self._SpecClass
        dyn_cls_args = dict()
        for port_name, value_type in self.Inputs.items():
            dyn_cls_args[port_name] = (InPortType[value_type], ...)
        for port_name, value_type in self.Outputs.items():
            dyn_cls_args[port_name] = (OutPortType[value_type], ...)
        #
        cls_name = camelize_string(self.Name)
        self._SpecClass = pydantic.create_model(cls_name + 'NodeSpec', **dyn_cls_args)
        return self._SpecClass


class PortsAccessor(object):
    def __init__(self, node: 'AbilityGraphNode', node_type: typing.Type[AbilityNodeSpec], is_out: bool):
        self._node = node
        self._ports = {}
        for fname, fdef in node_type.__fields__.items():
            t_origin = typing.get_origin(fdef.outer_type_)
            if t_origin is InPortType or t_origin is OutPortType:
                port_is_out = t_origin is OutPortType
                if port_is_out == is_out:
                    port_cls = OutPort if is_out else InPort
                    t_arg = typing.get_args(fdef.outer_type_)[0]
                    self._ports[fname] = port_cls(node, fname, t_arg)

    def __getattr__(self, item) -> PortBase:
        return self._ports[item]

    def __iter__(self):
        return self._ports.values()


class AbilityGraphNode(object):
    def __init__(self, node_type: typing.Type[AbilityNodeSpec], graph: 'AbilityGraph', _id: int):
        self._id = _id
        self._node_type = node_type
        self._inputs = PortsAccessor(self, node_type, False)
        self._outputs = PortsAccessor(self, node_type, True)
        self._graph = graph

    @property
    def node_type(self) -> typing.Type[AbilityNodeSpec]:
        return self._node_type

    @property
    def id(self) -> int:
        return self._id

    @property
    def graph(self) -> 'AbilityGraph':
        return self._graph

    @property
    def inputs(self) -> PortsAccessor:
        return self._inputs

    @property
    def outputs(self) -> PortsAccessor:
        return self._outputs

    def to_dict(self) -> dict:
        return dict(
            Id=str(self._id),
            NodeType=''
        )


class AbilityGraphEdge(object):
    def __init__(self, src: PortBase, dst: PortBase):
        self._src = src
        self._dst = dst

    @property
    def src(self) -> PortBase:
        return self._src

    @property
    def dst(self) -> PortBase:
        return self._dst

    @property
    def key(self) -> typing.Tuple[int, int]:
        return self._src.id, self._dst.id

    def to_dict(self) -> dict:
        return dict(
            SrcNode=self._src.id,
            SrcPort=self._src.name,
            DstNode=self._dst.id,
            DstPort=self._dst.name)


class AbilityGraph(object):
    def __init__(self, name: str):
        self._graph = nx.DiGraph()
        self._name = name
        self._nodes: typing.List[AbilityGraphNode] = []
        self._edges = {}

    @property
    def name(self) -> str:
        return self._name

    def new_node(self, node_type: typing.Type[AbilityNodeSpec]) -> AbilityGraphNode:
        node_id = len(self._nodes)
        n = AbilityGraphNode(node_type, graph=self, _id=node_id)
        self._nodes.append(n)
        for inport in n.inputs:
            self._graph.add_node(inport.id)
        for outport in n.outputs:
            self._graph.add_node(outport.id)
        return n

    def connect_nodes(self, src: PortBase, dst: PortBase):
        if src.is_input or dst.is_output:
            raise AbilityGraphException()
        in_edges = list(self._graph.in_edges(nbunch=dst.id))
        if len(in_edges) > 0:
            raise AbilityGraphException()
        self._graph.add_edge(src.id, dst.id)
        e = AbilityGraphEdge(src, dst)
        self._edges[e.key] = e
        return e

    def to_dict(self) -> dict:
        return dict(
            Name=self._name,
            Nodes=[n.to_dict() for n in self._nodes],
            Edges=[e.to_dict() for e in self._edges.values()]
        )


class AbilityGraphNodeDTO(pydantic.BaseModel):
    Id: str
    NodeType: str


class AbilityGraphEdgeDTO(pydantic.BaseModel):
    SrcNode: str
    SrcPort: str
    DstNode: str
    DstPort: str


class AbilityGraphDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    Nodes: typing.List[AbilityGraphNodeDTO]
    Edges: typing.List[AbilityGraphEdgeDTO]


class AbilitySystem(_BaseModel):
    Name: str

    def add_actor(self, _type: typing.Type[DataModel]):
        pass

    def to_dict(self):
        pass
