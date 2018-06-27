#include "Havok.h"
#include <Common/Base/keycode.cxx>
#include <Common/Base/Config/hkProductFeatures.cxx>
#include <cstdio>
#include <vector>
#include <iostream>
#include <fstream>
#include <string>
#include <algorithm>

static void HK_CALL errorReport(const char* msg, void* userArgGivenToInit)
{
	printf("%s", msg);
}

unsigned int getUserDataFromName(std::string name) {
	unsigned int flags = 0, type = 0;
	int tagIndex = -1;
	size_t len = name.length();
	bool lastChar = false;

	std::transform(name.begin(), name.end(), name.begin(), ::tolower);

	for (size_t i = 0; i < len; ++i) {
		if (name[i] == '@' || (lastChar = (i == len - 1))) {
			if (tagIndex != -1) {
				std::string tag = name.substr(tagIndex,
					((lastChar) ? i + 1 : i) - tagIndex);

				// Flags
				if (tag == "not_stand") flags |= 1;
				else if (tag == "slide") flags |= 2;
				else if (tag == "breakable") flags |= 4;
				else if (tag == "stairs") flags |= 8;
				else if (tag == "parkour") flags |= 0x10;
				else if (tag == "walljump") flags |= 0x20;
				else if (tag == "not_ground") flags |= 0x800;
				else if (tag == "press_dead") flags |= 0x1000;
				else if (tag == "movable") flags |= 0x2000;
				else if (tag == "rayblock") flags |= 0x4000;
				else if (tag == "slip") flags |= 0x8000;

				// Type
				else if (tag == "stone") type = 0x00000000;
				else if (tag == "earth") type = 0x01000000;
				else if (tag == "wood") type = 0x02000000;
				else if (tag == "grass") type = 0x03000000;
				else if (tag == "iron") type = 0x04000000;
				else if (tag == "sand") type = 0x05000000;
				else if (tag == "phantomcube") type = 0x06000000;
				else if (tag == "ford") type = 0x08000000;
				else if (tag == "glass") type = 0x0a000000;
				else if (tag == "snow") type = 0x0b000000;
				else if (tag == "sandfall") type = 0x0e000000;
				else if (tag == "ice") type = 0x10000000;
				else if (tag == "water") type = 0x11000000;
				else if (tag == "sea") type = 0x12000000;
				else if (tag == "waterfall") type = 0x13000000;
				else if (tag == "dead") type = 0x17000000;
				else if (tag == "waterdead") type = 0x18000000;
				else if (tag == "damage") type = 0x1a000000;
				else if (tag == "pool") type = 0x1c000000;
				else if (tag == "air") type = 0x1d000000;
				else if (tag == "invisible") type = 0x1f000000;
				else if (tag == "wiremesh") type = 0x20000000;
				else if (tag == "dead_anydir") type = 0x24000000;
				else if (tag == "damage_through") type = 0x25000000;
				else if (tag == "dry_grass") type = 0x26000000;
				else if (tag == "wetroad") type = 0x27000000;
				else if (tag == "snake") type = 0x28000000;
			}

			tagIndex = i + 1;
		}
	}

	return type | flags;
}

void rigidBodiesToCompoundShape(const char* filename, const char* output)
{
	// Load the file
	hkSerializeUtil::ErrorDetails loadError;
	hkResource* loadedData = hkSerializeUtil::load(filename, &loadError);
	if ( !loadedData )
	{
		{
			HK_ASSERT2(0xa6451543, m_loadedData != HK_NULL, "Could not load file. The error is:\n" << loadError.defaultMessage.cString() );
		}
	}

	// Get the top level object in the file, which we know is a hkRootLevelContainer
	hkRootLevelContainer* container = loadedData->getContents<hkRootLevelContainer>();
	HK_ASSERT2(0xa6451543, container != HK_NULL, "Could not load root level obejct" );

	// Get the physics data
	hkpPhysicsData* physicsData = static_cast<hkpPhysicsData*>( container->findObjectByType( hkpPhysicsDataClass.getName() ) );
	HK_ASSERT2(0xa6451544, physicsData != HK_NULL, "Could not find physics data in root level object" );
	HK_ASSERT2(0xa6451535, physicsData->getWorldCinfo() != HK_NULL, "No physics cinfo in loaded file - cannot create a hkpWorld" );
	
	// Create the hkpStaticCompoundShape and add the instances.
	// "meshShape" should not be modified by the user in any way after adding it as an instance.
	std::vector<std::string> names;
	hkpStaticCompoundShape* staticCompoundShape = new hkpStaticCompoundShape();

	for ( int i = 0; i < physicsData->getPhysicsSystems().getSize(); ++i )
	{
		hkpPhysicsSystem* physicsSystem = physicsData->getPhysicsSystems()[i];
		for ( int j = 0; j < physicsSystem->getRigidBodies().getSize(); ++j )
		{
			hkpRigidBody* rigidBody = physicsSystem->getRigidBodies()[j]; 
			hkpRigidBodyCinfo info;
			rigidBody->getCinfo(info);
			names.push_back(rigidBody->getName());

			hkQsTransform transform(hkQsTransform::IDENTITY);

			hkVector4 position = info.m_position;
			position.mul(10);
			transform.setTranslation(position);

			transform.setRotation(info.m_rotation);
			transform.setScale(hkVector4(10, 10, 10, 10));
			
			// Get geometry from shape.
			hkGeometry *geometry = hkpShapeConverter::toSingleGeometry(info.m_shape);
			unsigned int userData = getUserDataFromName(rigidBody->getName());
			
			for (int t = 0; t < geometry->m_triangles.getSize(); t++) {
				geometry->m_triangles[t].m_material = userData;
		    }
			
			// Construct hkBvCompressedMeshShape
			hkpDefaultBvCompressedMeshShapeCinfo cinfo(geometry);
			cinfo.m_collisionFilterInfoMode = hkpBvCompressedMeshShape::PerPrimitiveDataMode::PER_PRIMITIVE_DATA_PALETTE;
			cinfo.m_userDataMode = hkpBvCompressedMeshShape::PerPrimitiveDataMode::PER_PRIMITIVE_DATA_PALETTE;
			
            hkpShape *shape = new hkpBvCompressedMeshShape(cinfo);
			shape->setUserData(userData);

			int instanceId = staticCompoundShape->addInstance(shape, transform);
			staticCompoundShape->setInstanceUserData(instanceId, 1042652845 + names.size() - 1);
			staticCompoundShape->setInstanceFilterInfo(instanceId, 11);
		}
	}    

	// This must be called after adding the instances and before using the shape.
	staticCompoundShape->bake();
	
	hkRootLevelContainer::NamedVariant compoundData("shape", (void*)staticCompoundShape, &hkpStaticCompoundShapeClass);
	hkRootLevelContainer *compoundContainer = new hkRootLevelContainer();
	compoundContainer->m_namedVariants.pushBack(compoundData);

	hkSerializeUtil::SaveOptions options;
	hkPackfileWriter::Options packFileOptions;
	packFileOptions.m_layout = hkStructureLayout::MsvcWin32LayoutRules;
	// Save for PC
	hkSerializeUtil::savePackfile( compoundContainer, hkRootLevelContainerClass, hkOstream(output).getStreamWriter(), packFileOptions );
	
	delete compoundContainer;
	delete staticCompoundShape;
}

int main(int argc, char **argv) {
	hkMemoryRouter* memoryRouter = hkMemoryInitUtil::initDefault( hkMallocAllocator::m_defaultMallocAllocator,
	hkMemorySystem::FrameInfo( 500* 1024 ) );
	hkBaseSystem::init( memoryRouter, errorReport );

	rigidBodiesToCompoundShape(argv[1], argc > 2 ? argv[2] : argv[1]);

	hkBaseSystem::quit();
	hkMemoryInitUtil::quit();

	return 0;
}