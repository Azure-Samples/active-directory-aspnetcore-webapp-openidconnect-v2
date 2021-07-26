#### 1. On Windows run PowerShell and navigate to the <solution's folder>/AppCreationScripts
#### 2. In PowerShell run:

```PowerShell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
```
#### 3. _**While you're in <solution's folder>/AppCreationScripts**_, run the script to create your Azure AD application and configure the code of the sample application accordingly
```PowerShell
.\Configure.ps1
```

       > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

#### 4. Open the Visual Studio solution and click start. That's it!
