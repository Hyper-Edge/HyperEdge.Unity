import certifi
import json
import os
import pydantic
import ssl
import sys
import time
import multiprocessing
import threading
import websocket

from hyperedge.sdk.unipipe import send_to_unity


class JobData(pydantic.BaseModel):
    job_id: str
    success: bool
    retval: object


def _get_ws_job_id(v):
    if not v:
        return None
    parts = v.split('.')
    return parts[1] if len(parts) == 2 else None


def _ws_listener(ws, queue: multiprocessing.Queue):
    while True:
        try:
            msg = ws.recv()
            if not msg:
                continue
            jmsg = json.loads(msg)
            print(jmsg, flush=True)
            if jmsg['event'] == 'message':
                job_id = _get_ws_job_id(jmsg.get('subscription'))
                if not job_id:
                    assert False
                    continue
                if 'task' in jmsg['data']:
                    #print("sending to unity", flush=True)
                    send_to_unity('HeTaskInfo', {
                        'JobId': job_id,
                        'Task': jmsg['data']['task'],
                        'Payload': jmsg['data'].get('payload'),
                    })
                    #print("sent", flush=True)
                elif 'status' in jmsg['data']:
                    is_success = jmsg['data']['status'] == 'success'
                    job_data = JobData(job_id=job_id, success=is_success, retval=jmsg['data'].get('retval'))
                    queue.put(job_data)
        except websocket.WebSocketConnectionClosedException:
            # handle connection closed, probably break the loop
            print("Connection closed")
            sys.exit(0)
        except Exception as e:
            print("Other error occured. {}".format(e))
            sys.exit(0)


class HeWsClient(object):
    def __init__(self, url: str, ticket: str):
        self._url = url
        self._mgr = multiprocessing.Manager()
        self._queue = self._mgr.Queue()
        #
        self._ssl_ctxt = ssl.SSLContext(ssl.PROTOCOL_TLS_CLIENT)
        self._ssl_ctxt.check_hostname = True
        self._ssl_ctxt.verify_mode = ssl.CERT_REQUIRED
        self._ssl_ctxt.load_verify_locations(certifi.where())
        time.sleep(3)
        self._ws = websocket.create_connection(self._url, sslopt={
            'context': self._ssl_ctxt
        })
        self._thread = threading.Thread(target=_ws_listener, args=(self._ws, self._queue,), daemon=True)
        self._thread.start()
        self._job_results = {}
        #
        self.auth(ticket)

    @property
    def url(self):
        return self._url

    def auth(self, ticket):
        self._ws.send(json.dumps({'event': 'auth', 'method': 'ticket', 'ticket': ticket}))

    def _subscribe_for_job(self, job_id):
        self._ws.send(json.dumps({"event": "subscribe", "subscription": f"jobs.{job_id}"}))

    def wait_for_job(self, job_id: str):
        self._subscribe_for_job(job_id)
        #
        job_data = self._job_results.get(job_id)
        while job_data is None:
            q_job_data = self._queue.get()
            self._job_results[q_job_data.job_id] = q_job_data
            job_data = self._job_results.get(job_id)
        return job_data
