.PHONY: *
CID := $(shell cat /tmp/prism.cid)

docs:
	doxygen

restore:
	dotnet restore Lucidtech

build: restore
	msbuild Lucidtech/Lucidtech.csproj

restore-test:
	dotnet restore Test

build-test: restore-test
	msbuild Test/Test.csproj

test: build-test
	cd Test	&& mono $(HOME)/.nuget/packages/nunit.consolerunner/3.11.1/tools/nunit3-console.exe --stoponerror ./bin/Debug/net461/Test.dll
	cd Test	&& mono $(HOME)/.nuget/packages/nunit.consolerunner/3.11.1/tools/nunit3-console.exe --stoponerror ./bin/Debug/net47/Test.dll
	cd Test	&& mono $(HOME)/.nuget/packages/nunit.consolerunner/3.11.1/tools/nunit3-console.exe --stoponerror ./bin/Debug/net472/Test.dll

single-test: build-test
	if [ -z "$(name)" ]; then echo 'you need to specify the name of your test ex. name=TestDeleteBatch' && exit 1; fi
	cd Test && mono $(HOME)/.nuget/packages/nunit.consolerunner/3.11.1/tools/nunit3-console.exe --stoponerror ./bin/Debug/net472/Test.dll --test=Test.TestClient.$(name)

prism-start:
	@echo "Starting mock API..."
	docker run -t \
		--init \
		--detach \
		-p 4010:4010 \
		stoplight/prism:4.11.0 mock -d -h 0.0.0.0 \
		https://raw.githubusercontent.com/LucidtechAI/cradl-docs/master/static/oas.json \
		> /tmp/prism.cid

prism-stop:
ifeq ("$(wildcard /tmp/prism.cid)","")
	@echo "Nothing to stop."
else
	docker stop $(CID)
endif
