#if defined(_WIN32)
#include <stdio.h>
#include <windows.h>
#include "string.h"
#include "setjmp.h"
#elif defined(__linux__)
#include "stdio.h"
#include "string.h"
#include <unistd.h>
#endif

extern "C"
{
	unsigned long long getAvailableSystemMemory_Windows64();
	unsigned long long getAvailableSystemMemory_Linux64();
}

#if defined(_WIN32)

__declspec(dllexport) extern unsigned long long getAvailableSystemMemory_Windows64()
{

	MEMORYSTATUSEX status;
	status.dwLength = sizeof(status);
	GlobalMemoryStatusEx(&status);
	return status.ullAvailPhys / 1024 / 1024;
}

#elif defined(__linux__)

extern unsigned long long getAvailableSystemMemory_Linux64()
{

	unsigned long long ps = sysconf(_SC_PAGESIZE);
	unsigned long long pn = sysconf(_SC_AVPHYS_PAGES);
	unsigned long long availMem = ps * pn;
	return availMem / 1024 / 1024;
}

#endif

int main()
{
#if defined(_WIN32)
	printf("%d", getAvailableSystemMemory_Windows64());
#elif defined(__linux__)
	printf("%d", getAvailableSystemMemory_Linux64());

#endif
	printf("MB");

	int a;
	scanf("This is the value %d", &a);
}




// for Windows compile it with: g++ --shared  -o SystemInfoCpp.dll  hello.cpp
// for Linux compile it with: g++ -o SystemInfoCpp.so hello.cpp --shared -fPIC

