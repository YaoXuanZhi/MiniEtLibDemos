### 恢复工程并编译

```sh
dotnet restore
dotnet build
```

### 运行全部测试用例

```sh
dotnet test --logger:"console;verbosity=minimal"
```

### 运行指定模块测试用例

```sh
dotnet test  --logger:"console;verbosity=minimal" --filter "TestCase_Login|TestCase_Gm"
```