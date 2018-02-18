import struct

import sys
import os

import xml.etree.cElementTree as ET

import subprocess
   
class TagSubType(object):
    Void = 0x0
    Invalid = 0x1
    Bool = 0x2
    String = 0x3
    Int = 0x4
    Float = 0x5
    Pointer = 0x6
    Class = 0x7
    Array = 0x8
    Tuple = 0x28
    TypeMask = 0xff
    IsSigned = 0x200
    Float32 = 0x1746
    Int8 = 0x2000
    Int16 = 0x4000
    Int32 = 0x8000
    Int64 = 0x10000
    
class TagFlag(object):
    SubType = 0x1
    Pointer = 0x2
    Version = 0x4
    ByteSize = 0x8
    AbstractValue = 0x10
    Members = 0x20
    Interfaces = 0x40
    Unknown = 0x80
    
class TagMember(object):
    def __init__(self):
        self.name = ""
        self.flags = 0
        self.byteOffset = 0
        self.typ = None
        
class TagTemplate(object):
    def __init__(self, name = "v", value = 0):
        self.name = name
        self.value = value
        
    @property
    def isInt(self):
        return self.name[0] == "v"
        
    @property
    def isType(self):
        return self.name[0] == "t"
    
class TagType(object):
    def __init__(self, name = ""):
        self.name = name
        self.templates = []
        self.parent = None
        self.flags = 0
        self.subTypeFlags = 0
        self.pointer = None
        self.version = 0
        self.byteSize = 0
        self.alignment = 0
        self.abstractValue = 0
        self.members = []
        self.interfaces = []
        self.hsh = 0
        
    @property
    def superType(self):
        if not self.flags & TagFlag.SubType:
            return self.parent.superType
            
        else:
            return self
        
    @property
    def subType(self):
        return self.subTypeFlags & TagSubType.TypeMask

    @property
    def allMembers(self):
        if self.parent:
            for member in self.parent.allMembers:
                yield member
                
        for member in self.members:
            yield member
            
    @property
    def tupleSize(self):
        return self.subTypeFlags >> 8

class TagObject(object):
    def __init__(self, value, typ):
        self.value = value
        self.typ = typ
        self.attachment = None
        
class TagItem(object):
    def __init__(self):
        self.typ = None
        self.offset = 0
        self.count = 0
        self.isPtr = False
        self.value = None
        
class TagSectionReader(object):
    def __init__(self, r, signature):
        self.r = r
        self.offset = r.f.tell() + 8
        self.size = (r.readFormat(">I") & 0x3FFFFFFF) - 8
        
        if r.f.read(4) != signature:
            raise ValueError()
    
    @property
    def end(self):
        return self.r.f.tell() >= (self.offset + self.size)
            
    def __enter__(self):
        self.r.f.seek(self.offset)
        return self
        
    def __exit__(self, arg1, arg2, arg3):
        self.r.f.seek(self.offset + self.size)

class TagReader(object):
    def __init__(self, f):
        self.f = f
        self.dataOffset = 0
        self.types = []
        self.items = []
        self.readRootSection()
        
    def __enter__(self):
        return self
        
    def __exit__(self, arg1, arg2, arg3):
        self.f.close()
    
    @staticmethod    
    def fromFile(inputFileName, typeName = "hkRootLevelContainer"):
        with TagReader( open(inputFileName, "rb") ) as r:
            return r.getObject(typeName)

    def readTypeSection(self):
        with TagSectionReader(self, "TYPE") as t1:
            with TagSectionReader(self, "TPTR") as t2:
                pass
                
            with TagSectionReader(self, "TSTR") as t3:
                typeStrings = self.f.read(t3.size).split("\0")
                
            with TagSectionReader(self, "TNAM") as t4:
                typeCount = self.readPacked()
                self.types = [TagType() for x in xrange(typeCount)]
                self.types[0] = None
                
                for typ in self.types[1:]:
                    typ.name = typeStrings[self.readPacked()]
                    
                    for i in xrange( self.readPacked() ):
                        template = TagTemplate( typeStrings[ self.readPacked() ], self.readPacked() )
                   
                        if template.isType:
                            template.value = self.types[template.value]
                            
                        typ.templates.append(template)
                    
            with TagSectionReader(self, "FSTR") as t5:
                fieldStrings = self.f.read(t5.size).split("\0")
                
            with TagSectionReader(self, "TBOD") as t6:
                while not t6.end:
                    typeIndex = self.readPacked()
                    
                    if typeIndex == 0:
                        continue
                        
                    typ = self.types[typeIndex]
                    typ.parent = self.types[self.readPacked()]
                    typ.flags = self.readPacked()
                    
                    if typ.flags & TagFlag.SubType:
                        typ.subTypeFlags = self.readPacked()
                        
                    if typ.flags & TagFlag.Pointer and typ.subTypeFlags & 0xF >= 6:
                        typ.pointer = self.types[self.readPacked()]
                        
                    if typ.flags & TagFlag.Version:
                        typ.version = self.readPacked()
                        
                    if typ.flags & TagFlag.ByteSize:
                        typ.byteSize = self.readPacked()
                        typ.alignment = self.readPacked()
                        
                    if typ.flags & TagFlag.AbstractValue:
                        typ.abstractValue = self.readPacked()
                    
                    if typ.flags & TagFlag.Members:
                        for i in xrange( self.readPacked() ):
                            member = TagMember()
                            member.name = fieldStrings[self.readPacked()]
                            member.flags = self.readPacked()
                            member.byteOffset = self.readPacked()
                            member.typ = self.types[self.readPacked()]
                            typ.members.append(member)
                            
                    if typ.flags & TagFlag.Interfaces:
                        typ.interfaces = [
                        	(self.types[self.readPacked()], self.readPacked())
                        	for x in xrange(self.readPacked())]
                        	
                    if typ.flags & TagFlag.Unknown:
                        raise ValueError("Flag 0x80 exists, handle it!")
            
            with TagSectionReader(self, "THSH") as t7:
                for i in xrange( self.readPacked() ):
                    typeIndex = self.readPacked()
                    self.types[typeIndex].hsh = self.readFormat("<I")
                    
            with TagSectionReader(self, "TPAD") as t8:
                pass
            
    def readIndexSection(self):
        with TagSectionReader(self, "INDX") as t1:
            with TagSectionReader(self, "ITEM") as t2:
                while not t2.end:
                    item = TagItem()
                    flag = self.readFormat("<I")
                    item.typ = self.types[flag & 0xFFFFFF]
                    item.isPtr = bool(flag & 0x10000000)
                    item.offset = self.dataOffset + self.readFormat("<I")
                    item.count = self.readFormat("<I")
                    self.items.append(item)
                    
            with TagSectionReader(self, "PTCH") as t3:
                pass

    def readRootSection(self):
        with TagSectionReader(self, "TAG0") as t1:
            
            with TagSectionReader(self, "SDKV") as t2:
                if self.f.read(8) != "20160100":
                    raise ValueError("Invalid SDK version.")
                    
            with TagSectionReader(self, "DATA") as t3:
                self.dataOffset = t3.offset
                
            self.readTypeSection()
            self.readIndexSection()
    
    @staticmethod
    def getFormatString(flags, signed = False):
        ret = ""
        
        if flags & TagSubType.Int8:
            ret = "B"
        
        elif flags & TagSubType.Int16:
            ret = "<H"
            
        elif flags & TagSubType.Int32:
            ret = "<I"
            
        elif flags & TagSubType.Int64:
            ret = "<Q"
            
        if flags & TagSubType.IsSigned or signed:
            return ret.lower()
            
        else:
            return ret
    
    def readObject(self, typ, offset = 0):
        if offset == 0:
            offset = self.f.tell()
            
        else:
            self.f.seek(offset)
            
        typOrg = typ
        typ = typ.superType
        
        value = None
        
        if typ.subType == TagSubType.Bool:
            value = self.readFormat( TagReader.getFormatString(typ.subTypeFlags) ) > 0
            
        elif typ.subType == TagSubType.String:
            value = "".join( map(chr, [x.value for x in self.readItemPtr()[:-1]]) )
            
        elif typ.subType == TagSubType.Int:
            value = self.readFormat( TagReader.getFormatString(typ.subTypeFlags) )
        
        elif typ.subType == TagSubType.Float:
            value = self.readFormat("<f")
        
        elif typ.subType == TagSubType.Pointer:
            value = self.readItemPtr()
            
            if len(value) == 1:
                value = value[0]
                
            else:
                value = None
        
        elif typ.subType == TagSubType.Class:
            value = {x.name:self.readObject(x.typ, offset + x.byteOffset)
            	for x in typ.allMembers}
            	
        elif typ.subType == TagSubType.Array:
            value = self.readItemPtr()
            
        elif typ.subType == TagSubType.Tuple:
            value = tuple([self.readObject(typ.pointer, offset + x * typ.pointer.superType.byteSize)
            	for x in xrange(typ.tupleSize)])
            	
        self.f.seek(offset + typ.byteSize)
        return TagObject(value, typOrg)
        
    def readItemPtr(self):
        index = self.readFormat("<I")
        
        if index == 0:
            return []
            
        else:
            item = self.items[index]
            
            if item.value == None:
                item.value = [self.readObject(item.typ,
                	item.offset + x * item.typ.superType.byteSize)
                	for x in xrange(item.count)]
                
            return item.value

    def readFormat(self, format):
        data = struct.unpack(format,
        	self.f.read( struct.calcsize(format) ))

        if len(data) == 1:
            return data[0]

        else:
            return data

    def readPacked(self):
        byte = self.readFormat("B")

        if byte & 0x80:
            
            if byte & 0x40:
                
                if byte & 0x20:
                    return (byte << 24 | self.readFormat("B") << 16 | self.readFormat(">H")) & 0x7ffffff
                
                else:
                    return (byte << 16 | self.readFormat(">H")) & 0x1fffff
            
            else:
                return (byte << 8 | self.readFormat("B")) & 0x3fff
        
        else:
            return byte
            
    def getType(self, name):
        for typ in self.types[1:]:
            if typ.name == name:
                return typ
            
    def getItem(self, typ):
        if isinstance(typ, str):
            typ = self.getType(typ)
            
        for item in self.items:
            if item.typ == typ:
                return item
            
    def getObject(self, typ = "hkRootLevelContainer"):
        item = self.getItem(typ)
        
        if item.typ == None:
            return None
        
        if item.value == None:
            item.value = [self.readObject(item.typ,
            	item.offset + x * item.typ.superType.byteSize)
            	for x in xrange(item.count)]
            
        return item.value[0]
        
class TagSectionWriter(object):
    def __init__(self, w, signature, flag = True):
        self.w = w
        self.headerOffset = w.f.tell()
        self.flag = flag
        
        w.writeFormat(">I", 0)
        w.f.write(signature[:4])
        
    def __enter__(self):
        self.w.f.seek(self.headerOffset + 8)
        return self
        
    def __exit__(self, arg1, arg2, arg3):    
        self.w.pad(4)
        
        endOffset = self.w.f.tell()
        
        self.w.f.seek(self.headerOffset)
        if self.flag:
            self.w.writeFormat(">I", 0x40000000 | (endOffset - self.headerOffset))
        else:
            self.w.writeFormat(">I", endOffset - self.headerOffset)
            
        self.w.f.seek(endOffset)
        
class TagWriter(object):
    def __init__(self, f):
        self.f = f
        self.dataOffset = 0
        self.types = [None]
        self.items = [None]
        self.items2 = []
        self.patches = {}
        
    def __enter__(self):
        return self
        
    def __exit__(self, arg1, arg2, arg3):
        self.f.close()
        
    @staticmethod
    def toFile(outputFileName, obj):
        with TagWriter(open(outputFileName, "wb")) as w:
            w.writeRootSection(obj)
        
    def writeTypeSection(self):
        with TagSectionWriter(self, "TYPE", False) as t1:
            
            with TagSectionWriter(self, "TPTR") as t2:
                self.writeNulls(8 * len(self.types))
                
            typeStrings = []
            fieldStrings = []
            for typ in self.types[1:]:
                if not typ.name in typeStrings:
                    typeStrings.append(typ.name)
                    
                for template in typ.templates:
                    if not template.name in typeStrings:
                        typeStrings.append(template.name)
                        
                for member in typ.members:
                    if not member.name in fieldStrings:
                        fieldStrings.append(member.name)
                        
            with TagSectionWriter(self, "TSTR") as t3:
                self.f.write("\0".join(typeStrings) + "\0")

            with TagSectionWriter(self, "TNAM") as t4:
                self.writePacked( len(self.types) )
                
                for typ in self.types[1:]:
                    self.writePacked(typeStrings.index(typ.name))
                    self.writePacked( len(typ.templates) )
                    
                    for template in typ.templates:
                        self.writePacked(typeStrings.index(template.name))
                        self.writePacked(self.types.index(template.value) if template.isType else template.value)
                
            with TagSectionWriter(self, "FSTR") as t5:
                self.f.write("\0".join(fieldStrings) + "\0")
                
            with TagSectionWriter(self, "TBOD") as t6:
                for typ in self.types[1:]:
                    self.writePacked(self.types.index(typ))
                    self.writePacked(self.types.index(typ.parent))
                    self.writePacked(typ.flags)
                    
                    if typ.flags & TagFlag.SubType:
                        self.writePacked(typ.subTypeFlags)
                        
                    if typ.flags & TagFlag.Pointer:
                        self.writePacked(self.types.index(typ.pointer))
                        
                    if typ.flags & TagFlag.Version:
                        self.writePacked(typ.version)
                        
                    if typ.flags & TagFlag.ByteSize:
                        self.writePacked(typ.byteSize)
                        self.writePacked(typ.alignment)
                        
                    if typ.flags & TagFlag.AbstractValue:
                        self.writePacked(typ.abstractValue)
                        
                    if typ.flags & TagFlag.Members:
                        self.writePacked( len(typ.members) )
                        
                        for member in typ.members:
                            self.writePacked(fieldStrings.index(member.name))
                            self.writePacked(member.flags)
                            self.writePacked(member.byteOffset)
                            self.writePacked(self.types.index(member.typ))
                            
                    if typ.flags & TagFlag.Interfaces:
                        self.writePacked( len(typ.interfaces) )
                        
                        for typ, flag in typ.interfaces:
                            self.writePacked(self.types.index(typ))
                            self.writePacked(flag)    
     
            with TagSectionWriter(self, "THSH") as t7:
                hashes = [x for x in self.types[1:] if x.hsh]
                
                self.writePacked( len(hashes) )
                
                for typ in hashes:
                    self.writePacked(self.types.index(typ))
                    self.writeFormat("<I", typ.hsh)
                    
            with TagSectionWriter(self, "TPAD") as t8:
                pass

    def writeIndexSection(self):
        with TagSectionWriter(self, "INDX", False) as t1:
            
            with TagSectionWriter(self, "ITEM") as t2:
                self.writeNulls(12)
                
                for item in self.items[1:]:
                    if item.isPtr:
                        self.writeFormat("<I", self.types.index(item.typ) | 0x10000000)
                    else:
                        self.writeFormat("<I", self.types.index(item.typ) | 0x20000000)
                   
                    self.writeFormat("<I", item.offset - self.dataOffset)
                    self.writeFormat("<I", len(item.value))
                   
            with TagSectionWriter(self, "PTCH") as t3:
                patches = [(self.types.index(key), value)
                	for key, value in self.patches.iteritems()]
               	
                patches.sort(key=lambda x: x[0])
               
                for typ, offsets in patches:
                    offsets = list( set(offsets) )
                    offsets.sort()
                    
                    self.writeFormat("<2I", typ, len(offsets))
                    
                    for offset in offsets:
                        self.writeFormat("<I", offset - self.dataOffset)

    def writeRootSection(self, obj):
        self.scanObjectForType(obj)
        self.makeItem(obj, True)
    
        with TagSectionWriter(self, "TAG0", False) as t1:
        
            with TagSectionWriter(self, "SDKV") as t2:
                self.f.write("20160100")
                
            with TagSectionWriter(self, "DATA") as t3:
                self.dataOffset = t3.headerOffset + 8
                
                while len(self.items2):
                    items3 = self.items2
                    self.items2 = []
                    
                    for item in items3:
                        self.pad(2)
                        self.pad(item.typ.superType.alignment)
                        
                        item.offset = self.f.tell()
                        for i in xrange( len(item.value) ):
                            self.writeObject(item.value[i], item.offset + i * item.typ.superType.byteSize)
                        
                self.pad(16)
                
            self.writeTypeSection()
            self.writeIndexSection()
        
    def writeObject(self, obj, offset = 0):
        if offset == 0:
            offset = self.f.tell()
            
        else:
            self.f.seek(offset)
            
        typ = obj.typ.superType
        
        if typ.subType == TagSubType.Bool:
            self.writeFormat(TagReader.getFormatString(typ.subTypeFlags), obj.value)
            
        elif typ.subType == TagSubType.String or typ.subType == TagSubType.Pointer or typ.subType == TagSubType.Array:
            	
            	item = self.makeItem(obj)
            	if item != None:
            	    self.addPatch(typ)
            	    self.writeFormat("<I", self.items.index(item))
            
        elif typ.subType == TagSubType.Int:
            self.writeFormat(TagReader.getFormatString(typ.subTypeFlags, obj.value < 0), obj.value)
        
        elif typ.subType == TagSubType.Float:
            self.writeFormat("<f", obj.value)
        
        elif typ.subType == TagSubType.Class:
            for member in typ.allMembers:
                if obj.value.has_key(member.name):
                    self.writeObject(obj.value[member.name], offset + member.byteOffset)
            	
        elif typ.subType == TagSubType.Tuple:
            for i in xrange(typ.tupleSize):
                self.writeObject(obj.value[i], offset + i * typ.pointer.superType.byteSize)
            
        self.f.seek(offset + typ.byteSize)
        
    def addPatch(self, typ):
        if self.patches.has_key(typ):
            self.patches[typ].append(self.f.tell())
            
        else:
            self.patches[typ] = [self.f.tell()]
        
    def writeFormat(self, format, *args):
        self.f.write(struct.pack(format, *args))
        
    def writePacked(self, value):
        if value < 0x80:
            self.writeFormat("B", value)
        elif value < 0x4000:
            self.writeFormat(">H", value | 0x8000)
        elif value < 0x200000:
            self.writeFormat("B", (value >> 16) | 0xc0)
            self.writeFormat(">H", value & 0xffff)
        elif value < 0x8000000:
            self.writeFormat(">I", value | 0xe0000000)
        
    def writeNulls(self, amount):
        self.f.write("\0" * amount)
        
    def pad(self, alignment):
        amount = alignment - self.f.tell() % alignment
        
        if amount != alignment:
            self.writeNulls(amount)
        
    def makeItem(self, obj, pointer = False):
        if obj.value == None or (hasattr(obj.value, "__len__") and len(obj.value) <= 0):
            return None
            
        if obj.attachment != None:
            return obj.attachment
            
        item = TagItem()
        
        if obj.typ.superType.subType == TagSubType.String:
            item.typ = self.getType("char")
            item.value = [TagObject(ord(x), item.typ) for x in obj.value + "\0"]
            
        elif obj.typ.superType.subType == TagSubType.Pointer or pointer:
            # Fake Pointer
            if obj.typ.superType.subType == TagSubType.Class:
                item.typ = obj.typ
                item.value = [obj]
                item.isPtr = True
                
            else:
                item.typ = obj.value.typ
                item.value = [obj.value]
                item.isPtr = True
            
        elif obj.typ.superType.subType == TagSubType.Array:
            item.typ = obj.typ.superType.pointer
            item.value = obj.value
            
            if item.typ.superType.subType == TagSubType.Pointer:
                item.isPtr = True
                
        else:
            return None
            
        obj.attachment = item
        
        self.items.append(item)
        self.items2.append(item)
        
        return item
    
    def scanType(self, typ):
        if typ != None and not typ in self.types:
            self.types.append(typ)            
            
            for template in typ.templates:
                if template.isType:
                    self.scanType(template.value)
                
            self.scanType(typ.parent)
            self.scanType(typ.pointer)
            
            for member in typ.members:
                self.scanType(member.typ)
        
            for iTyp, flag in typ.interfaces:
                self.scanType(iTyp)
    
    def scanObjectForType(self, obj):
        if obj == None:
            return
    
        self.scanType(obj.typ)
            
        if obj.typ.superType.subType == TagSubType.Pointer:
            self.scanObjectForType(obj.value)
        
        elif obj.typ.superType.subType == TagSubType.Class:
            for member in obj.typ.allMembers:
                if obj.value.has_key(member.name):
                    self.scanObjectForType(obj.value[member.name])
                    
        elif obj.typ.superType.subType & 0xF == TagSubType.Array:
            for obj2 in obj.value:
                self.scanObjectForType(obj2)
                        
    def getType(self, name):
        for typ in self.types[1:]:
            if typ.name == name:
                return typ
                
class TagTypeHelper(object):
    @staticmethod
    def getAttrib(elem, name, default = None, method = None):
        if elem.attrib.has_key(name):
            attribValue = elem.attrib[name]
            
            if method != None:
                return method(attribValue)
                
            else:
                return attribValue
                
        else:
            return default

    @staticmethod
    def loadTypes(inputFileName):
        rootElem = ET.parse(inputFileName)
        typeElems = list( rootElem.findall("type") )
        typeElems.sort(key = lambda x: int( x.get("id") ))
        types = [None] + [TagType() for x in typeElems]
        
        for i in xrange(1, len(types) ):
            typ = types[i]
            typeElem = typeElems[i - 1]
            
            typ.name = TagTypeHelper.getAttrib(typeElem, "name", "")
            typ.parent = types[TagTypeHelper.getAttrib(typeElem, "parent", 0, int)]
            typ.flags = TagTypeHelper.getAttrib(typeElem, "flags", 0, int)
            typ.subTypeFlags = TagTypeHelper.getAttrib(typeElem, "subTypeFlags", 0, int)
            typ.pointer = types[TagTypeHelper.getAttrib(typeElem, "pointer", 0, int)]
            typ.version = TagTypeHelper.getAttrib(typeElem, "version", 0, int)
            typ.byteSize = TagTypeHelper.getAttrib(typeElem, "byteSize", 0, int)
            typ.alignment = TagTypeHelper.getAttrib(typeElem, "alignment", 0, int)
            typ.abstractValue = TagTypeHelper.getAttrib(typeElem, "abstractValue", 0, int)
            typ.hsh = TagTypeHelper.getAttrib(typeElem, "hash", 0, int)
            
            for tempElem in typeElem.findall("template"):
                template = TagTemplate(
                	TagTypeHelper.getAttrib(tempElem, "name", "v"),
                	TagTypeHelper.getAttrib(tempElem, "value", 0, int) )
                	
                if template.isType:
                    template.value = types[template.value]
                    
                typ.templates.append(template)
                
            for memberElem in typeElem.findall("member"):
                member = TagMember()
                member.name = TagTypeHelper.getAttrib(memberElem, "name", "")
                member.flags = TagTypeHelper.getAttrib(memberElem, "flags", 0, int)
                member.byteOffset = TagTypeHelper.getAttrib(memberElem, "offset", 0, int)
                member.typ = types[TagTypeHelper.getAttrib(memberElem, "type", 0, int)]
                typ.members.append(member)
                    
            for interfaceElem in typeElem.findall("interface"):
                typ.interfaces.append((
                	types[TagTypeHelper.getAttrib(interfaceElem, "type", 0, int)],
                	TagTypeHelper.getAttrib(interfaceElem, "flags", 0, int)))
            
        return types[1:]
        
class TagXmlParser(object):
    def __init__(self, rootElem, types):
        self.types = types
        self.objectElems = list( rootElem.findall("object") )
        self.objects = [None] + [TagObject(None, None) for x in xrange( len(self.objectElems) )]
        self.objectElems.sort(key = lambda x: self.parseObjId( x.get("id") ))
    
    @staticmethod
    def fromFile(inputFileName, types, objName = "hkRootLevelContainer"):
        return TagXmlParser(ET.parse(inputFileName), types).findObject(objName)
    
    def findType(self, name):
        name = name.replace("::", "")
    
        for typ in self.types:
            if typ and typ.name.replace("::", "") == name:
                return typ
                
    def findObject(self, name):
        if isinstance(name, TagType):
            name = name.name
            
        name = name.replace("::", "")
    
        for i in xrange( len(self.objectElems) ):    
            if self.objectElems[i].get("type") == name:
                return self.parseObject(i + 1)
    
    def parseObjId(self, val):
        if val.startswith("#"):
            return int(val[1:])
            
        return 0
        
    def parseFloat(self, text):
        return struct.unpack("f", struct.pack("I", int(text[1:], 16)))[0]
        
    def splitNumArray(self, text):
        prettyString = text.strip().replace("\n", "").replace("\r", "")
        return [x for x in prettyString.split(" ") if x]
        
    def parseNumArray(self, typ, text):
        return [self.parseValueText(typ, x) for x in self.splitNumArray(text)]
        
    def parseArray(self, typ, elem):
        pointer = typ.superType.pointer.superType
        
        value = None
        if pointer.subType >= TagSubType.Bool and pointer.subType <= TagSubType.Float and pointer.subType != TagSubType.String:
            value = self.parseNumArray(typ.superType.pointer, elem.text)
            
        else:
            value = [self.parseValue(typ.superType.pointer, x) for x in elem]
            
        return TagObject([x for x in value if x], typ)
        
    def parseValueText(self, typ, text):
        value = None
        
        if typ.superType.subType == TagSubType.Bool:    
            value = bool(text)
    
        elif typ.superType.subType == TagSubType.Int:
            if typ.superType.subTypeFlags & TagSubType.Int64:
                value = long(text)
            else:
                value = int(text)
            
        elif typ.superType.subType == TagSubType.Float:
            value = self.parseFloat(text)
        
        return TagObject(value, typ)
        
    def parseValue(self, typ, elem):
        if typ.superType.subType == TagSubType.String:    
            return TagObject(elem.text, typ)
    
        elif typ.superType.subType >= TagSubType.Bool and typ.superType.subType <= TagSubType.Float:
            return self.parseValueText(typ, elem.text)
            
        elif typ.superType.subType == TagSubType.Pointer:
            return TagObject(self.parseObject(self.parseObjId(elem.text)), typ)
            
        elif typ.superType.subType == TagSubType.Class:
            members = {x.name:x for x in typ.superType.allMembers}
            
            for memberElem in elem:
                name = memberElem.get("name")
                value = self.parseValue(members[name].typ, memberElem)
                
                # Any invalid type: death sentence
                if value == None or value.value == None or value.typ == None:
                    return None
                    
                members[name] = value
        
            if typ.superType.name == "hkQsTransformf":
                floats = [TagObject(self.parseFloat(x), self.findType("float")) for x in self.splitNumArray(elem.text)]
                
                members["translation"] = TagObject(floats[:4], members["translation"].typ)
                members["rotation"] = TagObject(floats[4:8], members["rotation"].typ)
                members["scale"] = TagObject(floats[8:12], members["scale"].typ)
        
            return TagObject({x:y for x, y in members.iteritems() if isinstance(y, TagObject)}, typ)
        
        elif typ.superType.subType & 0xF == TagSubType.Array:
            return self.parseArray(typ, elem)
        
    def parseObject(self, index):
        obj = self.objects[index]
        objElem = self.objectElems[index - 1]
        
        if obj.value == None or obj.typ == None:
            typ = self.findType( objElem.get("type") )
            
            if typ == None:    
                print "WARNING: Type '{}' could not be found in the type database!".format( objElem.get("type") )
                return
        
            obj2 = self.parseValue(typ, objElem)
            
            obj.value = obj2.value
            obj.typ = obj2.typ

        return obj
        
class TagXmlSerializer(object):
    def __init__(self):
        self.types = []
        self.objects = []
        self.objCounter = 0
        
    @staticmethod
    def toFile(outputFileName, obj):
        with open(outputFileName, "w") as f:
            f.write('<?xml version="1.0" encoding="ascii"?>\n')
            ET.ElementTree(TagXmlSerializer().serialize(obj)).write(f)
        
    def getIdString(self, index):
        return "#{:04}".format(index)
        
    def sanitizeTypeName(self, typ):
        typ = typ.superType
        
        name = typ.name
        for template in typ.templates:
            if template.isType:
                name += self.sanitizeTypeName(template.value)
            else:
                name += str(template.value)
        
        return name.replace(":", "").replace(" ", "")
        
    def getSubTypeName(self, typ):
        typ = typ.superType
    
        if typ.subType == TagSubType.Bool or typ.subType == TagSubType.Int:
            if typ.subTypeFlags & TagSubType.Int8:
                return "byte"
            else:
                return "int"
                
        elif typ.subType == TagSubType.String:
            return "string"
        
        elif typ.subType == TagSubType.Float:
            return "real"
        
        elif typ.subType == TagSubType.Pointer:
            return "ref"
        
        elif typ.subType == TagSubType.Class:
            return "struct"
        
        elif typ.subType == TagSubType.Array:
            return "array"
        
        elif typ.subType == TagSubType.Tuple:
            return "tuple"
        
        return ""
        
    def getFloatString(self, value):
        return "x{:08x}".format(struct.unpack("I", struct.pack("f", value))[0])
        
    def getValueString(self, obj):
        typ = obj.typ.superType
        
        if typ.subType == TagSubType.Bool:
            return str(1 if obj.value else 0)
            
        elif typ.subType == TagSubType.Int:
            return str(obj.value)
            
        elif typ.subType == TagSubType.Float:
            return self.getFloatString(obj.value)

    def makeNumArray(self, obj):
        index = 16 if obj.typ.superType.pointer.superType.byteSize == 1 else 8
        
        result = ""
        for i in xrange( len(obj.value) ):
            if not i % index:
                result += "\n"
        
            result += self.getValueString(obj.value[i]) + " "
                
        return result[:-1]

    def serializeObject(self, parent, obj):
        if (hasattr(obj.value, "__len__") and len(obj.value) > 0) or obj.value:
            elem = ET.SubElement(parent, self.getSubTypeName(obj.typ))
            
            typ = obj.typ.superType
            
            if typ.subType == TagSubType.Bool:
                elem.text = str(1 if obj.value else 0)
                
            elif typ.subType == TagSubType.String:
                elem.text = obj.value
                
            elif typ.subType == TagSubType.Int:
                elem.text = str(obj.value)
                
            elif typ.subType == TagSubType.Float:
                elem.text = self.getFloatString(obj.value)
                
            elif typ.subType == TagSubType.Pointer:
                elem.text = self.getIdString(obj.value.attachment)
                
            elif typ.subType == TagSubType.Class:
                # hkQsTransformf
                if typ.name == "hkQsTransformf":
                    floats = [x.value for x in
                    	obj.value["translation"].value + obj.value["rotation"].value + obj.value["scale"].value]
                    	
                    elem.tag = "vec12"
                    elem.text = " ".join([self.getFloatString(x) for x in floats])
                
                else:
                    for member in typ.allMembers:
                        if obj.value.has_key(member.name):
                            memberElem = self.serializeObject(elem, obj.value[member.name])
                            
                            if memberElem != None:
                                memberElem.set("name", member.name)
                                 
            elif typ.subType & 0xF == TagSubType.Array:
                pointer = typ.pointer.superType
                
                if pointer.subType == TagSubType.Bool or pointer.subType == TagSubType.Int:
                    elem.text = self.makeNumArray(obj)
                    
                elif pointer.subType == TagSubType.Float:
                    elem.text = " ".join([self.getFloatString(x.value) for x in obj.value])
        
                else:
                    for obj2 in obj.value:    
                        self.serializeObject(elem, obj2)
                        
                if typ.subType == TagSubType.Array:
                    elem.set("size", str( len(obj.value) ))
                    
                elif typ.subType == TagSubType.Tuple:
                    elem.set("size", str(typ.tupleSize))
                    
                # hkVector4
                if typ.tupleSize == 4 and pointer.subType == TagSubType.Float:
                    elem.tag = "vec4"
                    elem.attrib.pop("size")
        
            return elem
            
    def serializeMemberProp(self, parent, typ):
        if typ == None:
            return
    
        typ = typ.superType
        
        parent.set("type", self.getSubTypeName(typ))
        
        if typ.subType == TagSubType.Pointer:
            parent.set("class", self.sanitizeTypeName(typ.pointer))
            
        elif typ.subType == TagSubType.Class:
            if typ.name == "hkQsTransformf":
                parent.set("type", "vec12")
                
            else:
                parent.set("class", self.sanitizeTypeName(typ))
                
        elif typ.subType == TagSubType.Array:
            parent.set("array", "true")
            self.serializeMemberProp(parent, typ.pointer)
            
        elif typ.subType == TagSubType.Tuple:
            if typ.pointer.superType.subType == TagSubType.Float and typ.tupleSize == 4:
                parent.set("type", "vec4")
                
            else:
                parent.set("count", str(typ.tupleSize))
                self.serializeMemberProp(parent, typ.pointer)
            
    def serializeType(self, parent, typ):
        elem = ET.SubElement(parent, "class")
        elem.set("name", self.sanitizeTypeName(typ))
        elem.set("version", str(typ.version))
        
        if typ.parent != None:
            elem.set("parent", self.sanitizeTypeName(typ.parent))
            
        for member in typ.members:
            memberElem = ET.SubElement(elem, "member")
            memberElem.set("name", member.name)
            self.serializeMemberProp(memberElem, member.typ)
            
            if member.flags & 1:
                memberElem.set("type", "void")
            
        return elem
        
    def serialize(self, obj):
        self.objects.append(obj)
        self.objCounter += 1
        obj.attachment = self.objCounter                
        self.scanObjectForType(obj)
        
        rootElem = ET.Element("hktagfile", {"version":"1", "sdkversion":"hk_2012.2.0-r1"})
        
        for typ in self.types:
            if typ.subType == TagSubType.Class and typ.name != "hkQsTransformf":
                self.serializeType(rootElem, typ)
                
        for obj2 in self.objects:
            elem = self.serializeObject(rootElem, obj2)
            elem.set("id", self.getIdString(obj2.attachment))
            elem.set("type", self.sanitizeTypeName(obj2.typ.superType))
            elem.tag = "object"
          
        TagXmlSerializer.indent(rootElem)      
        return rootElem
        
    @staticmethod
    def indent(elem, level=0, hor='  ', ver='\n'):
        i = ver + level * hor
        if len(elem):
            if not elem.text or not elem.text.strip():
                elem.text = i + hor
            if not elem.tail or not elem.tail.strip():
                elem.tail = i
            for elem in elem:
                TagXmlSerializer.indent(elem, level + 1, hor, ver)
            if not elem.tail or not elem.tail.strip():
                elem.tail = i
        else:
            if level and (not elem.tail or not elem.tail.strip()):
                elem.tail = i
                if elem.text and elem.text.startswith("\n"):
                    elem.text = elem.text.replace("\n", i + hor) + i
        
    def scanType(self, typ):
        if typ != None and not typ in self.types:
            self.scanType(typ.parent)
            self.scanType(typ.pointer)
            
            self.types.append(typ)
            
            for member in typ.members:
                self.scanType(member.typ)
    
    def scanObjectForType(self, obj):
        if obj == None:
            return
    
        self.scanType(obj.typ)
        
        if obj.typ.superType.subType == TagSubType.Pointer and obj.value and not obj.value.attachment:
            self.objects.append(obj.value)
            self.objCounter += 1
            obj.value.attachment = self.objCounter
        
            self.scanObjectForType(obj.value)
        
        elif obj.typ.superType.subType == TagSubType.Class:
            for member in obj.typ.allMembers:
                if obj.value.has_key(member.name):
                    self.scanObjectForType(obj.value[member.name])
                    
        elif obj.typ.superType.subType & 0xF == TagSubType.Array:
            for obj2 in obj.value:
                self.scanObjectForType(obj2)
        
def findFile(fileName):
    for arg in sys.argv:
        path = os.path.join(os.path.dirname(arg), fileName)
        
        if os.path.exists(path):
            return path
            
    raise ValueError("{} could not be found.".format(fileName))
        
if __name__ == "__main__":
    if len(sys.argv) <= 1:
        print "Tool for converting HKX (version <= 2012 2.0) files to 2016 1.0 tag binary files."
        print "\nUsage: {} [source] [destination]".format(os.path.basename(sys.argv[0]))
        print "If no destination is included, the changes will be overwritten to the source."
        print "You can do a simple drag and drop that way."
        print "\nMade by Skyth."
        print "Press enter to continue..."
        raw_input()
    
    else:
        inputFileName = sys.argv[1]
        if len(sys.argv) >= 3:
            outputFileName = sys.argv[2]
        else:
            outputFileName = inputFileName
            
        if False:
            with open(inputFileName, "rb") as f:
                f.seek(4)
                
                if f.read(4) == "TAG0":
                    f.seek(0)
                    
                    r = TagReader(f)
                    
                    if inputFileName == outputFileName:
                        outputFileName = outputFileName[:outputFileName.rindex(".")] + ".xml"
                    
                    TagXmlSerializer.toFile(outputFileName, r.getObject())
                    
                    sys.exit()
            
        types = TagTypeHelper.loadTypes(findFile("TypeDatabase.xml"))
        tempFilePath = os.path.join(os.path.dirname(sys.argv[0]), "temp.xml")
        subprocess.call([findFile("AssetCc2.exe"), "-g", "-x", inputFileName, tempFilePath])
        TagWriter.toFile(outputFileName, TagXmlParser.fromFile(tempFilePath, types))
        os.remove(tempFilePath)