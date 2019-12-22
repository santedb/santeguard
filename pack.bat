@ECHO OFF


	FOR /D %%G IN (.\*) DO (
		PUSHD %%G
		IF EXIST "manifest.xml" (
			sdb-pakr --compile --optimize --source=.\ --output=..\bin\org.santedb.sg.pak --keyFile=..\..\keys\community.santesuite.net.pfx --keyPassword=..\..\keys\community.santesuite.net.pass --embedcert
		)
		POPD
	)

PUSHD bin

copy ..\..\applets\bin\org.santedb.core.pak /y
copy ..\..\applets\bin\org.santedb.bicore.pak /y
copy ..\..\applets\bin\org.santedb.uicore.pak /y
copy ..\..\applets\bin\org.santedb.config.pak /y
copy ..\..\applets\bin\org.santedb.admin.pak /y
copy ..\..\applets\bin\org.santedb.i18n.en.pak /y
sdb-pakr --compose --source=..\santeguard.sln.xml -o santeguard.sln.pak --keyFile=..\..\keys\community.santesuite.net.pfx --embedCert --keyPassword=..\..\keys\community.santesuite.net.pass --embedcert
POPD

