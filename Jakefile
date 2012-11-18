var fs = require('fs'),
    path = require('path'),
    njake = require('./src/njake'),
    msbuild = njake.msbuild,
    nuget = njake.nuget,
    assemblyInfo = njake.assemblyInfo,
    config = {
        rootPath: __dirname,
        version: fs.readFileSync('VERSION', 'utf-8')
    };

console.log('Nancy.Facebook.RealtimeSubscription v' + config.version)

msbuild.setDefaults({
    properties: { Configuration: 'Release' },
    processor: 'x86',
    version: 'net4.0'
})

nuget.setDefaults({
    _exe: 'src/.nuget/NuGet.exe',
    verbose: true
})

assemblyInfo.setDefaults({
    language: 'c#'
})

task('default', ['build'])

desc('Build')
task('build', ['assemblyInfo'], function () {
    msbuild({
        file: 'src/Nancy.Facebook.RealtimeSubscription.sln',
        targets: ['Build']
    })
}, { async: true })

task('clean', function () {
    msbuild({
        file: 'src/Nancy.Facebook.RealtimeSubscription.sln',
        targets: ['Clean']
    })
}, { async: true })

task('assemblyInfo', function () {
    assemblyInfo({
        file: 'src/Nancy.Facebook.RealtimeSubscription/Properties/AssemblyInfo.cs',
        assembly: {
            notice: function () {
                return '// Do not modify this file manually, use jakefile instead.\r\n';
            },
            AssemblyTitle: 'Nancy.Facebook.RealtimeSubscription',
            AssemblyDescription: 'Nancy.Facebook.RealtimeSubscription',
            AssemblyCompany: 'Prabir Shrestha',
            AssemblyProduct: 'Nancy.Facebook.RealtimeSubscription',
            AssemblyCopyright: 'Copyright (c) 2012, Prabir Shrestha.',
            ComVisible: false,
            AssemblyVersion: config.version,
            AssemblyFileVersion: config.version
        }
    })
}, { async: true })
