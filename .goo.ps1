<# goo.ps1 - Type less. Code more.

    Develop, build, test and run helper script built on Powershell

    Developed by Andre Sharpe on October, 24 2020.

    www.goo.dev

    1. '.\.goo' will output the comment headers for each implemented command
    
    2. Add a function with its purpose in its comment header to extend this project's goo file 

    3. 'goo <function>' will run your commands 
#>

<# --- NEW GOO INSTANCE --- #>

using module '.\.goo\goo.psm1'

$goo = [Goo]::new($args)


<# --- SET GLOBAL SCRIPT VARIABLES HERE --- #>

$script:SolutionName            = 'Nox.Cron'

$script:RootFolder              = (Get-Location).Path
$script:SourceFolder            = '.\src'
$script:SolutionFolder          = $script:SourceFolder
$script:SolutionFile            = "$script:SolutionFolder\Nox.Cron.sln"
$script:ProjectFolder           = "$script:SourceFolder\Nox.Cron"
$script:ProjectFile             = "$script:ProjectFolder\Nox.Cron.csproj"
$script:TestFolder              = "$script:SourceFolder\Nox.Cron.Tests"

$script:DefaultEnvironment      = 'Development'

<# --- SET YOUR PROJECT'S ENVIRONMENT VARIABLES HERE --- #>

if($null -eq $Env:Environment)
{
    $Env:ENVIRONMENT = $script:DefaultEnvironment
    $Env:ASPNETCORE_ENVIRONMENT = $script:DefaultEnvironment
}


<# --- ADD YOUR COMMAND DEFINITIONS HERE --- #>

<# 
    A good 'init' command will ensure a freshly cloned project will run first time.
    Guide the developer to do so easily. Check for required tools. Install them if needed. Set magic environment variables if needed.
    This should ideally replace your "Getting Started" section in your README.md
    Type less. Code more. (And get your team or collaboraters started quickly and productively!)
#>

# command: goo init | Run this command first, or to reset project completely. 
$goo.Command.Add( 'init', {
    $goo.Command.Run( 'clean' )
    $goo.Command.Run( 'build' )
    $goo.Command.Run( 'run' )
})

# command: goo clean | Removes data and build output
$goo.Command.Add( 'clean', {
    $goo.Console.WriteInfo( "Cleaning data and distribution folders..." )
    $goo.IO.EnsureRemoveFolder("./dist")
    $goo.IO.EnsureRemoveFolder("./src/dist")
    $goo.Command.RunExternal('dotnet','clean --verbosity:quiet --nologo',$script:SolutionFolder)
    $goo.Command.RunExternal('dotnet','restore --verbosity:quiet --nologo',$script:SolutionFolder)
    $goo.StopIfError("Failed to clean previous builds. (Release)")
})


# command: goo build | Builds the solution and command line app. 
$goo.Command.Add( 'build', {
    $goo.Console.WriteInfo("Building solution...")
    $goo.Command.RunExternal('dotnet','build /clp:ErrorsOnly --configuration Release', $script:SolutionFolder)
    $goo.StopIfError("Failed to build solution. (Release)")
    $goo.Command.RunExternal('dotnet','publish --configuration Release --output ..\dist --no-build', $script:ProjectFolder )
    $goo.StopIfError("Failed to publish CLI project. (Release)")
})

# command: goo env | Show all environment variables
$goo.Command.Add( 'env', { param($dbEnvironment,$dbInstance)
    $goo.Console.WriteLine( "environment variables" )
    $goo.Console.WriteLine( "=====================" )
    Get-ChildItem -Path Env: | Sort-Object -Property Name | Out-Host

    $goo.Console.WriteLine( "dotnet user-secrets" )
    $goo.Console.WriteLine( "===================" )
    $goo.Console.WriteLine() 
    $goo.Command.RunExternal('dotnet',"user-secrets list --project $script:ProjectFile")
})

# command: goo setenv <env> | Sets local environment to <env> environment
$goo.Command.Add( 'setenv', { param( $Environment )
    $oldEnv = $Env:ENVIRONMENT
    $Env:ENVIRONMENT = $Environment
    $Env:ASPNETCORE_ENVIRONMENT = $Environment
    $goo.Console.WriteInfo("Environment changed from [$oldEnv] to [$Env:ENVIRONMENT]")
})

# command: goo dev | Start up Visual Studio and VS Code for solution
$goo.Command.Add( 'dev', { 
    $goo.Command.StartProcess($script:SolutionFile)
    $goo.Command.StartProcess('code','.')
})

# command: goo run | Run the console application
$goo.Command.Add( 'run', {
    $goo.Command.RunExternal('dotnet','test',$script:TestFolder)
})

# command: goo feature <name> | Creates a new feature branch from your main git branch
$goo.Command.Add( 'feature', { param( $featureName )
    $goo.Git.CheckoutFeature($featureName)
})

# command: goo push <message> | Performs 'git add -A', 'git commit -m <message>', 'git -u push origin'
$goo.Command.Add( 'push', { param( $message )
    $current = $goo.Git.CurrentBranch()
    $head = $goo.Git.HeadBranch()
    if($head -eq $current) {
        $goo.Error("You can't push directly to the '$head' branch")
    }
    else {
        $goo.Git.AddCommitPushRemote($message)
    }
})

# command: goo pr | Performs and merges a pull request, checkout main and publish'
$goo.Command.Add( 'pr', { 
    gh pr create --fill
    if($?) { gh pr merge --merge }
    $goo.Command.Run( 'main' )
})

# command: goo main | Checks out the main branch and prunes features removed at origin
$goo.Command.Add( 'main', { param( $featureName )
    $goo.Git.CheckoutMain()
})

# command: goo publish | Build and publish Nox.Cron nuget package
$goo.Command.Add( 'publish', { 
    
    $goo.Console.WriteInfo("Updating version for ($script:ProjectFolder) and dependancies...")
    $goo.Command.Run( 'bump-project-version' )

    $goo.Console.WriteInfo("Compiling project ($script:ProjectFolder)...")
    $goo.Command.RunExternal('dotnet','build /clp:ErrorsOnly --warnaserror --configuration Release', $script:ProjectFolder)
    $goo.StopIfError("Failed to build solution. (Release)")

    $goo.Console.WriteInfo("Packing project ($script:ProjectFolder)...")
    $goo.Command.RunExternal('dotnet','pack /clp:ErrorsOnly --no-build --configuration Release', $script:ProjectFolder)
    $goo.StopIfError("Failed to pack solution (Release)")

    $goo.Console.WriteInfo("Publishing project ($script:ProjectFolder) to Nuget.org...")
    $nupkgFile = Get-ChildItem "$script:ProjectFolder\bin\Release\$script:SolutionName.*.nupkg" | Sort-Object -Property LastWriteTime | Select-Object -Last 1
    $goo.Command.RunExternal('dotnet',"nuget push $($nupkgFile.FullName) --api-key $Env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json", $script:ProjectFolder)
    $goo.StopIfError("Failed to publish $script:ProjectFolder to nuget. (Release)")

})

$goo.Command.Add( 'bump-project-version', {
    $files = (Get-ChildItem "*.csproj" -Recurse)
    $xpaths = @(
        "//AssemblyVersion",
        "//FileVersion",
        "//PackageVersion"
    )

    $xml = New-Object XML
    foreach($file in $files){
        $updated = $false
        $versionNew = $null
        $xml.Load($file)
        foreach($p in $xpaths){ 
            $node = $xml.SelectSingleNode($p)
            if($null -ne $node){
                $version = (($node.InnerText ?? $node.Value) -split '\.')
                $version[2] = [int]($version[2])+1
                $versionNew = ($version -join '.')
                $node.InnerText = $versionNew
                $updated = $true
        }
        }
        if ($updated) {
            $goo.Console.WriteLine("Bumping version for $($file.Name) to $versionNew..." )
            $xml.Save($file)
        }
    }
})


<# --- START GOO EXECUTION --- #>

$goo.Start()


<# --- EOF --- #>
