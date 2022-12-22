import sqlite3, sys, os
from glob import glob

class DB:
  def __init__(self):
    if os.path.isfile('cinema.db'):
        os.remove('cinema.db')

    with open("desc.json") as f:
      desc = eval(f.read())

    schema = "CREATE TABLE CDB ("
    fields = "("
    format = "("
    selection = ""
    first = True;
    if 'sliders' in desc:
      for slider in desc['sliders']:
        if not first:
          schema = schema + ",\n"
          fields = fields + ', '
          format = format + ","
          selection = selection + " AND "
        schema = schema + slider["name"] + " INT"
        fields = fields + slider["name"]
        format = format + " %d"
        selection = selection  + slider["name"] + "=%s"
        first = False
    if 'radio_buttons' in desc:
      for radio_button in desc['radio_buttons']:
        if not first:
          schema = schema + ",\n"
          fields = fields + ', '
          format = format + ","
          selection = selection + " AND "
        schema = schema + radio_button["name"] + " CHAR[50]"
        fields = fields + radio_button["name"]
        format = format + " '%s'"
        selection = selection  + radio_button["name"] + "='%s'"
        first = False
      
    schema = schema + ",\ndbname char[256]);\n";
    fields = fields + ", dbname)"
    format = format + ", '%s')"
    self.selection = selection
      
    self.DB = sqlite3.connect("cinema.DB")
    self.DB.execute(schema)
    
    for db in glob("*db"):
      if 'head' in db:
        face = 'head'
      else:
        face = 'back'
      angle = db[len('mountain_%scurve' % face):-3]
      for ts in glob('%s/timestep*' % db):
        time = int(ts.split('_')[-1])
        s = "INSERT INTO CDB %s VALUES %s" % (fields, format % (time, angle, face, ts))
        self.DB.execute(s)
  
  def find(self, Q):
    print(self.selection)
    s = "SELECT dbname FROM CDB WHERE %s" % (self.selection % Q)
    print(s)
    cursor = self.DB.execute(s)
    for l in cursor:
      return l
