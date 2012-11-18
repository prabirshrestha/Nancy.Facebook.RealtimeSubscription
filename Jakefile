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

directory('dist/')

task('default', ['build', 'nuget:pack'])

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

directory('dist/nuget/', ['dist/'])
directory('dist/symbols/', ['dist/'])

namespace('nuget', function () {

    namespace('pack', function () {

        task('nuget', ['dist/nuget/', 'build'], function () {
            nuget.pack({
                nuspec: 'src/nuspecs/Nancy.Facebook.RealtimeSubscription.nuspec',
                version: config.version,
                outputDirectory: 'dist/nuget/'
            })
        }, { async: true })


        task('symbolsource', ['dist/symbols/', 'build'], function () {
            nuget.pack({
                nuspec: 'src/nuspecs/Nancy.Facebook.RealtimeSubscription.symbols.nuspec',
                version: config.version,
                outputDirectory: 'dist/symbols/'
            })
        }, { async: true })

        task('all', ['nuget:pack:nuget', 'nuget:pack:symbolsource'])

    })

    namespace('push', function () {

        desc('Push nuget package to nuget.org')
        task('nuget', function(apiKey) {
            nuget.push({
                apiKey: apiKey,
                package: path.join(config.rootPath, 'dist/nuget/Nancy.Facebook.RealtimeSubscription.' + config.fileVersion + '.nupkg')
            })
        }, { async: true })

        desc('Push nuget package to symbolsource')
        task('symbols', function(apiKey) {
            nuget.push({
                apiKey: apiKey,
                package: path.join(config.rootPath, 'dist/symbols/Nancy.Facebook.RealtimeSubscription.' + config.fileVersion + '.nupkg'),
                source: nuget.sources.symbolSource
            })
        }, { async: true })

    })

    desc('Create NuGet pacakges')
    task('pack', ['nuget:pack:all'])

})
