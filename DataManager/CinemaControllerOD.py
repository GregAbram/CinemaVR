import sys, os, pdb
from glob import glob
from socket import socket, AF_INET, SOCK_STREAM
from struct import pack
from time import sleep
from select import select
import threading
import time

HOST = "127.0.0.1"
DATAPORT = 1900
REQUESTPORT = 1901

S = 0
E = 999999

current_dset = "XXX"
current_tstep = -1
last_dset = "XXX"
last_tstep = -1

up = 'update'.encode()
lup = pack('!i', len(up))


if len(sys.argv) == 1:
  print("need to kniw upper limit on timesteps!")
  sys.exit(0)

max_timesteps = int(sys.argv[1])

dataset_names = glob('*.db')
info = ','.join([str(max_timesteps)] + dataset_names)
b = bytes(info, 'utf-8')
print("ready... info =", info, b)

def LoadTimestep(dset, tstep):
    timestep_dir = '%s/timestep_%04d*' % (dset, tstep)
    print("timestep_dir", timestep_dir)
    keyfiles = glob('%s/*.dat' % timestep_dir)
    timestep = {}
    for keyfile in keyfiles:
      with open(keyfile, 'rb') as f:
        timestep[keyfile.split('/')[-1][:-4]] = f.read()
    print(dset, tstep, timestep.keys())
    return timestep

def SendUpdate():
  with socket(AF_INET, SOCK_STREAM) as s:
    s.connect((HOST, DATAPORT))
    s.send(lup)
    s.send(up)
    ack = b'none'
    knt = 0
    while ack != b'ok' and knt < 5:
      knt = knt + 1
      try:
        r,w,e = select([s], [], [s], 5)
        bytes = s.recv(4)
        sz = int.from_bytes(bytes, byteorder='big')
        ack = s.recv(sz)
      except OSError as error:
        print(error)
        r = 0
    if ack != b'ok' and knt >= 5:
      print('failed to get update ack')
  print('updated')

def SendTimestep(timestep):
  for key in timestep:
    keydata = timestep[key]
    with socket(AF_INET, SOCK_STREAM) as s:
      s.connect((HOST, DATAPORT))
      s.sendall(keydata)
      bsz = s.recv(4)
      sz = int.from_bytes(bsz, byteorder='big')
      msg = s.recv(sz)
      if msg != b'ok':
        print('OK error (', msg, ') for', key) 
        sys.exit(1)
    print('sent', key)
  SendUpdate()

lock = threading.Lock()
value = 0

def listener():
  server = socket(AF_INET, SOCK_STREAM)
  server.setblocking(0)
  server.bind((HOST, REQUESTPORT))
  server.listen()
  while True:
    sleep(0.10)
    with lock:
      rwe = select([server], [], [], 0.10)
      if len(rwe[0]) > 0:
        conn, addr = server.accept()
        with conn:
          global current_tstep, current_dset
          conn.setblocking(1)
          bin = conn.recv(999)
          print(bin)
          if bin == b'info':
            print("INFO REQUEST!")
            conn.sendall(bytes(info, 'utf-8'))
          else:
            current_dset,s = bin.decode("utf-8").split(':')
            current_tstep = int(s)
            print(current_dset, current_tstep)
            conn.sendall(b'ok')

x = threading.Thread(target=listener)
x.start()

while True:
  with lock:
      # print(current_tstep, 'x', last_tstep, 'b', current_dset, 'c', last_dset)
      if current_tstep != last_tstep or current_dset != last_dset:
          last_tstep = current_tstep
          last_dset = current_dset
          timestep = LoadTimestep(current_dset, current_tstep)
          SendTimestep(timestep)
      sleep(0.01)
