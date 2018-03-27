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

unsigned int getSurfaceTypeFromName(std::string name) {
	std::transform(name.begin(), name.end(), name.begin(), ::tolower);
	int tagIndex = name.find_last_of("@");
	
	if (tagIndex != std::string::npos) {
		std::string type = name.substr(tagIndex + 1);
		
		if (type == "stone") return 0x00000000;
        else if (type == "earth") return 0x01000000;
        else if (type == "wood") return 0x02000000;
        else if (type == "grass") return 0x03000000;
        else if (type == "iron") return 0x04000000;
        else if (type == "sand") return 0x05000000;
        else if (type == "phantomcube") return 0x06000000;
        else if (type == "ford") return 0x08000000;
        else if (type == "glass") return 0x0a000000;
        else if (type == "snow") return 0x0b000000;
        else if (type == "sandfall") return 0x0e000000;
        else if (type == "ice") return 0x10000000;
        else if (type == "water") return 0x11000000;
        else if (type == "sea") return 0x12000000;
        else if (type == "waterfall") return 0x13000000;
        else if (type == "dead") return 0x17000000;
        else if (type == "waterdead") return 0x18000000;
        else if (type == "damage") return 0x1a000000;
        else if (type == "pool") return 0x1c000000;
        else if (type == "air") return 0x1d000000;
        else if (type == "invisible") return 0x1f000000;
        else if (type == "wiremesh") return 0x20000000;
        else if (type == "dead_anydir") return 0x24000000;
        else if (type == "damage_through") return 0x25000000;
        else if (type == "dry_grass") return 0x26000000;
        else if (type == "wetroad") return 0x27000000;
        else if (type == "snake") return 0x28000000;
    }
    return 0;
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
			unsigned int userData = getSurfaceTypeFromName(rigidBody->getName());
			
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