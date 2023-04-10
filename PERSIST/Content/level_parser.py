import json
import sys

class Wall():
    length = 0
    height = 8
    x = 0
    y = 0
    def __init__(self, color):
        self.color = color
    def __str__(self):
        return str([self.x, self.y, self.length, self.height, self.color])

def convert(filename):
    try:
        f = open(filename)
    except:
        print("ERROR opening file", filename)
        return

    raw = json.load(f)
    
    raw = blockify(raw, 'walls')
    raw = blockify(raw, 'rooms')
            
    json_object = json.dumps(raw, separators=(',', ':'))

    with open("sample.json", "w") as outfile:
        outfile.write(json_object)
        
    return
        
def blockify(raw, name):
    width = raw['width']
    height = raw['height']

    for i in raw['layers']:
        if i['name'] == name:
            data = i['data']
            break
            
    width = int(width / 8)
    height = int(height / 8)

    new_data = []
    for i in range(height):
        new_data.append([])
        for j in range(width):
            new_data[i].append(data[j + (width * i)])
            
    rows = []
    for i in range(height):
        rows.append([])
    
    for i in range(len(new_data)):
        row = new_data[i]
        new = Wall(row[0])
        new.y = i * 8
        for j in range(len(row)):
            if row[j] == new.color:
                new.length += 8
            else:
                rows[i].append(new)
                new = Wall(row[j])
                new.y = i * 8
                new.length = 8
                new.x = j * 8
        rows[i].append(new)
    
    temp_walls = {}
    
    for i in rows:
        for wall in i:
            temp = str(wall.x) + " " + str(wall.length) + " " + str(wall.color)
            if temp in temp_walls.keys():
                temp_walls[temp].height += 8
            else:
                temp_walls[temp] = wall

    walls = []
    for i in temp_walls.keys():
        if temp_walls[i].color != -1:
            walls.append(temp_walls[i])
            
    cleaned_walls = []
    for i in walls:
        # temp = [i.x, i.x, i.length, i.height]
        # cleaned_walls.append(temp)
        cleaned_walls.append(i.x)
        cleaned_walls.append(i.y)
        cleaned_walls.append(i.length)
        cleaned_walls.append(i.height)
        
    for i in raw['layers']:
        if i['name'] == name:
            i['data'] = cleaned_walls
            break
            
    return raw

convert(sys.argv[1])