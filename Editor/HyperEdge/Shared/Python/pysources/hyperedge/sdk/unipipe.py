import json
import os
import socket
import struct


class UniPipe:
    def __init__(self):
        self._host, self._port = os.environ.get('UNIPIPE').split(':')
        self._pipe = None

    def __enter__(self):
        self._pipe = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._pipe.connect((self._host, int(self._port)))
        return self
    
    def __exit__(self, *args):
        self._pipe.close()
        self._pipe = None

    def write(self, obj):
        jmsg = json.dumps(obj)
        msg_len = struct.pack('I', len(jmsg))
        self._pipe.sendall(msg_len + jmsg.encode())


def send_to_unity(msg_type: str, data):
    with UniPipe() as pipe:
        pipe.write(dict(Type=msg_type, Payload=data))
#
