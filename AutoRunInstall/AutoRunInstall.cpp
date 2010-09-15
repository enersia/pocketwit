#include "stdafx.h"
#include <windows.h>
#include <ce_setup.h>

bool IsDotNet35();

codeINSTALL_INIT Install_Init(
    HWND        hwndParent,
    BOOL        fFirstCall, 
    BOOL        fPreviouslyInstalled,
    LPCTSTR     pszInstallDir
    )
{
    return codeINSTALL_INIT_CONTINUE;
}

codeINSTALL_EXIT Install_Exit(
    HWND    hwndParent,
    LPCTSTR pszInstallDir,
    WORD    cFailedDirs,
    WORD    cFailedFiles,
    WORD    cFailedRegKeys,
    WORD    cFailedRegVals,
    WORD    cFailedShortcuts
    )
{
	PROCESS_INFORMATION pi      = {0};
    codeINSTALL_EXIT    cie     = codeINSTALL_EXIT_DONE;
	BOOL bDotNet35;

	// We are provided with the installation folder the
	// user has installed the application into. So prepend
	// the name of the application we want to launch.
	TCHAR szPath[MAX_PATH];
	_tcscpy(szPath, pszInstallDir);
	_tcscat(szPath, _T("\\"));
	bDotNet35 = IsDotNet35();
	if(bDotNet35)
		_tcscat(szPath, _T("PockeTwit.exe"));
	else
		_tcscat(szPath, _T("CheckDotNet.exe"));

	// Refresh TodayScreen to show plugin
	PostMessage(HWND_BROADCAST, WM_WININICHANGE, 0xF2, 0);
	
	// Start the application, and don't wait for it to exit
    if (!CreateProcess(szPath, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, &pi) && bDotNet35)
	{
		MessageBox(GetForegroundWindow(), szPath, L"Failed to start PockeTwit", MB_OK);
	    cie = codeINSTALL_EXIT_UNINSTALL;
	}
	else
	{
		CloseHandle(pi.hThread);
		CloseHandle(pi.hProcess);
	}
	return cie;
}

bool IsDotNet35()
{
	bool ret = false;
	int lpType = 0;
	HKEY key;
	DWORD dwValSize = 50;
	LONG regret;
	LPTSTR lpValname = new TCHAR[50];
	regret = RegOpenKeyEx(HKEY_LOCAL_MACHINE, _T("SOFTWARE\\Microsoft\\.NETCompactFramework"), 0, KEY_QUERY_VALUE, &key);
	if (regret == 0)
	{
		int i = 0;
		while((regret = RegEnumValue(key, i++, lpValname, &dwValSize, NULL,NULL, NULL, NULL)) == 0)
		{
			lpValname[1] = 0;
			lpValname[3] = 0;
			int majVer = _ttoi(&lpValname[0]);
			int minVer = _ttoi(&lpValname[2]);
			if(majVer > 3 || (majVer == 3 && minVer >= 5))
			{
				ret = true;
				break;
			}
		}
		RegCloseKey(key);
	}
	return ret;
}



codeUNINSTALL_INIT Uninstall_Init(
    HWND        hwndParent,
    LPCTSTR     pszInstallDir
    )
{
	// Disable Today Plugin
	HKEY key;
	DWORD dwEnabled, dwRet;
	DWORD lpcbData = sizeof(dwEnabled);
	
	dwRet = RegOpenKeyEx(HKEY_LOCAL_MACHINE,_T("\\Software\\Microsoft\\Today\\Items\\PockeTwit"),0,0,&key);
	
	dwEnabled = 0;
	
	dwRet = RegSetValueEx(key,_T("Enabled"),0,REG_DWORD,(LPBYTE)&dwEnabled,sizeof(DWORD));
	
	RegFlushKey(key);

	RegCloseKey(key);

	PostMessage(HWND_BROADCAST, WM_WININICHANGE, 0xF2, 0);

	Sleep(1000);

    return codeUNINSTALL_INIT_CONTINUE;
}



codeUNINSTALL_EXIT Uninstall_Exit(
    HWND    hwndParent
    )
{
    return codeUNINSTALL_EXIT_DONE;
}

