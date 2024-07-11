#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include "virtual_desktops.h"

void
register_hotkeys() 
{
    // These should auto unregister once the program dies
    if (RegisterHotKey(NULL, 1, MOD_ALT | MOD_NOREPEAT, '1')) printf("Registered Alt + 1\n");
    if (RegisterHotKey(NULL, 2, MOD_ALT | MOD_NOREPEAT, '2')) printf("Registered Alt + 2\n");
    if (RegisterHotKey(NULL, 3, MOD_ALT | MOD_NOREPEAT, '3')) printf("Registered Alt + 3\n");
    if (RegisterHotKey(NULL, 4, MOD_ALT | MOD_NOREPEAT, '4')) printf("Registered Alt + 4\n");
    if (RegisterHotKey(NULL, 5, MOD_ALT | MOD_NOREPEAT, '5')) printf("Registered Alt + 5\n");
}

int
main(void) 
{
    register_hotkeys();
 
    MSG msg;
    while (GetMessage(&msg, NULL, WM_HOTKEY, WM_HOTKEY) != 0) {
        // TODO: handle hotkeys
    }

    return 0;
}
 
// HRESULT EnumVirtualDesktops(IVirtualDesktopManagerInternal *pDesktopManager)
// {
//     IObjectArray *pObjectArray = nullptr;
//     HRESULT hr = pDesktopManager->GetDesktops(&pObjectArray);
//  
//     if (SUCCEEDED(hr))
//     {
//         UINT count;
//         hr = pObjectArray->GetCount(&count);
//  
//         if (SUCCEEDED(hr))
//         {
//             for (UINT i = 0; i < count; i++)
//             {
//                 IVirtualDesktop *pDesktop = nullptr;
//  
//                 if (FAILED(pObjectArray->GetAt(i, __uuidof(IVirtualDesktop), (void**)&pDesktop)))
//                     continue;
//  
//                 GUID id = { 0 };
//  
//                 pDesktop->Release();
//             }
//         }
//  
//         pObjectArray->Release();
//     }
//  
//     return hr;
// }
//  
// HRESULT GetCurrentVirtualDesktop(IVirtualDesktopManagerInternal *pDesktopManager)
// {
//     IVirtualDesktop *pDesktop = nullptr;
//     HRESULT hr = pDesktopManager->GetCurrentDesktop(&pDesktop);
//  
//     if (SUCCEEDED(hr))
//     {
//         GUID id = { 0 };
//  
//         pDesktop->Release();
//     }
//  
//     return hr;
// }
//  
// HRESULT EnumAdjacentDesktops(IVirtualDesktopManagerInternal *pDesktopManager)
// {
//     IVirtualDesktop *pDesktop = nullptr;
//     HRESULT hr = pDesktopManager->GetCurrentDesktop(&pDesktop);
//  
//     if (SUCCEEDED(hr))
//     {
//         GUID id = { 0 };
//         IVirtualDesktop *pAdjacentDesktop = nullptr;
//         hr = pDesktopManager->GetAdjacentDesktop(pDesktop, AdjacentDesktop::LeftDirection, &pAdjacentDesktop);
//  
//         if (SUCCEEDED(hr))
//         {
//             pAdjacentDesktop->Release();
//         }
//  
//         id = { 0 };
//         pAdjacentDesktop = nullptr;
//         hr = pDesktopManager->GetAdjacentDesktop(pDesktop, AdjacentDesktop::RightDirection, &pAdjacentDesktop);
//  
//         if (SUCCEEDED(hr))
//         {
//             if (SUCCEEDED(pAdjacentDesktop->GetID(&id)))
//             pAdjacentDesktop->Release();
//         }
//  
//         pDesktop->Release();
//     }
//
//     return hr;
// }
//  
// HRESULT ManageVirtualDesktops(IVirtualDesktopManagerInternal *pDesktopManager)
// {
//     IVirtualDesktop *pDesktop = nullptr;
//     HRESULT hr = pDesktopManager->GetCurrentDesktop(&pDesktop);
//  
//     if (FAILED(hr))
//     {
//         return hr;
//     }
//  
//     IVirtualDesktop *pNewDesktop = nullptr;
//     hr = pDesktopManager->CreateDesktopW(&pNewDesktop);
//  
//     if (SUCCEEDED(hr))
//     {
//         GUID id;
//         hr = pNewDesktop->GetID(&id);
//  
//         if (FAILED(hr))
//         {
//             pNewDesktop->Release();
//             return hr;
//         }
//  
//         hr = pDesktopManager->SwitchDesktop(pNewDesktop);
//  
//         if (FAILED(hr))
//         {
//             pNewDesktop->Release();
//             return hr;
//         }
//  
//         if (SUCCEEDED(hr))
//         {
//             hr = pDesktopManager->RemoveDesktop(pNewDesktop, pDesktop);
//             pDesktop->Release();
//
//             if (FAILED(hr))
//             {
//                 pNewDesktop->Release();
//                 return hr;
//             }
//         }
//     }
//
//     return hr;
// }
//
// int 
// main(void)
// {
//     ::CoInitialize(NULL);
//  
//     IServiceProvider* pServiceProvider = nullptr;
//     HRESULT hr = ::CoCreateInstance(
//         CLSID_ImmersiveShell, NULL, CLSCTX_LOCAL_SERVER,
//         __uuidof(IServiceProvider), (PVOID*)&pServiceProvider);
//  
//     if (SUCCEEDED(hr))
//     {
//         IVirtualDesktopManagerInternal *pDesktopManagerInternal = nullptr;
//
//         hr = pServiceProvider->QueryService(
//             CLSID_VirtualDesktopAPI_Unknown, 
//             &pDesktopManagerInternal);
//
//         if (SUCCEEDED(hr))
//         {
//             EnumVirtualDesktops(pDesktopManagerInternal);
//             GetCurrentVirtualDesktop(pDesktopManagerInternal);
//             EnumAdjacentDesktops(pDesktopManagerInternal);
//             ManageVirtualDesktops(pDesktopManagerInternal);
//  
//             pDesktopManagerInternal->Release();
//             pDesktopManagerInternal = nullptr;
//         }
//  
//         IVirtualDesktopManager *pDesktopManager = nullptr;
//         hr = pServiceProvider->QueryService(
//             __uuidof(IVirtualDesktopManager),
//             &pDesktopManager);
//  
//         if (SUCCEEDED(hr))
//         {
//             GUID desktopId = { 0 };
//             hr = pDesktopManager->GetWindowDesktopId(GetConsoleWindow(), &desktopId);
//  
//             pDesktopManager->Release();
//             pDesktopManager = nullptr;
//         }
//  
//         pServiceProvider->Release();
//     }
//  
//     return 0;
// }
