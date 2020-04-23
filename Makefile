.PHONY: *
CID := $(shell cat /tmp/prism.cid)

restore:
	dotnet restore Lucidtech

build:
	msbuild Lucidtech/Lucidtech.csproj

restore-test:
	dotnet restore Test

build-test: restore-test
	msbuild Test/Test.csproj

test: build-test
	nunit3-console ./Test/bin/Debug/net461/Test.dll
	nunit3-console ./Test/bin/Debug/net47/Test.dll
	nunit3-console ./Test/bin/Debug/net472/Test.dll

prism-start:
	@echo "Starting mock API..."
	docker run \
		--init \
		--detach \
		-p 4010:4010 \
		stoplight/prism:3.2.8 mock -d -h 0.0.0.0 \
		https://raw.githubusercontent.com/LucidtechAI/las-docs/rest-api-docs/apis/dev/oas.json > /tmp/prism.cid

prism-stop:
ifeq ("$(wildcard /tmp/prism.cid)","")
	@echo "Nothing to stop."
else
	docker stop $(CID)
endif