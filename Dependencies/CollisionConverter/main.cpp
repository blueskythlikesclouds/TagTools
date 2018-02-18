#include "Havok.h"
#include <Common/Base/keycode.cxx>
#include <Common/Base/Config/hkProductFeatures.cxx>
#include <cstdio>
#include <vector>
#include <iostream>
#include <fstream>

static void HK_CALL errorReport(const char* msg, void* userArgGivenToInit)
{
	printf("%s", msg);
}


void rigidBodiesToCompoundShape(const char* filename)
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
			
			// Construct hkBvCompressedMeshShape
			hkpDefaultBvCompressedMeshShapeCinfo cinfo(geometry);
			hkpBvCompressedMeshShape *shape = new hkpBvCompressedMeshShape(cinfo);
			shape->setUserData(318767104 + names.size() - 1);

			staticCompoundShape->addInstance(shape, transform);
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
	hkSerializeUtil::savePackfile( compoundContainer, hkRootLevelContainerClass, hkOstream(filename).getStreamWriter(), packFileOptions );
	
	delete compoundContainer;
	delete staticCompoundShape;
}

int main(int argc, char **argv) {
	hkMemoryRouter* memoryRouter = hkMemoryInitUtil::initDefault( hkMallocAllocator::m_defaultMallocAllocator,
	hkMemorySystem::FrameInfo( 500* 1024 ) );
	hkBaseSystem::init( memoryRouter, errorReport );

	rigidBodiesToCompoundShape(argv[1]);

	hkBaseSystem::quit();
	hkMemoryInitUtil::quit();

	return 0;
}
