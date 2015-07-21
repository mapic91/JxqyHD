import sys

if len(sys.argv) < 3:
    print("SetExp.py file num")
    print("file - ini 文件路径")
    print("num  - 杀死多少个同等级的敌人可以升一级")
    exit(0)

numToLevelUp = int(sys.argv[2])
f = open(sys.argv[1])
lines = f.readlines()
f.close()
f = open(sys.argv[1] + ".out", "w")

i = 0
exp = 100
for line in lines:
    if line.find('LevelUpExp') != -1:
        f.write('LevelUpExp=' + str(exp) + '\n')
        i += 1
        exp += i * i * numToLevelUp
    else:
        f.write(line)