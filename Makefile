.PHONY: all clean publish build

all: clean build publish

build: 
	@dotnet build -c Release

publish: build
	@dotnet publish src/IPK25-chat/IPK25-chat.csproj -r linux-x64 -c Release -o . \
    -p:PublishSingleFile=true

clean:
	@rm -rf bin/ obj/ ipk25chat-client ipk25chat.exe *.pdb
