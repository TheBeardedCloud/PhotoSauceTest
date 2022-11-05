include(${CMAKE_CURRENT_LIST_DIR}/../libraries.cmake)

set(VCPKG_TARGET_ARCHITECTURE x64)
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)
set(VCPKG_BUILD_TYPE release)

set(VCPKG_CMAKE_SYSTEM_NAME Linux)

if(PORT IN_LIST _PKG_LIBS)
  set(VCPKG_LIBRARY_LINKAGE dynamic)
  set(VCPKG_FIXUP_ELF_RPATH ON)
endif()
