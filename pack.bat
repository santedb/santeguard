@ECHO OFF


	FOR /D %%G IN (.\*) DO (
		PUSHD %%G
		IF EXIST "manifest.xml" (
			pakman --compile --optimize --source=.\ --output=..\bin\org.santedb.sg.pak --keyFile=..\..\keys\community.santesuite.net.pfx --keyPassword=..\..\keys\community.santesuite.net.pass --embedcert --install
		)
		POPD
	)

mkdir dist
pakman --compose --source=.\santeguard.sln.xml -o dist\santeguard.sln.pak --keyFile=..\keys\community.santesuite.net.pfx --embedCert --keyPassword=..\keys\community.santesuite.net.pass --embedcert
POPD

