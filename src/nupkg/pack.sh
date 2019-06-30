#!/usr/bin/env bash

set -euo pipefail

usage()
{
    cat <<END
pack.sh: linux环境下打包surging组件脚本
Parameters:
    -r | --repo <nuget repo>
       nuget 仓库地址
    -p | --push <push>
       是否推送surging组件
    --ship-build <ship build>
       是否跳过build
    -k | --apikey <api key>
       nuget repo apikey
    -h | --help 
       显示帮助
END
}

nuget_repo=""
push=""
apikey=""
build="yes"
workdir=$(cd $(dirname $0); pwd)
slnPath="${workdir}/.."
srcPath="${workdir}/../src/Surging.Core"

while [[ $# -gt 0 ]]; do
  case "$1" in
    -r | --repo )
      nuget_repo="$2"; shift 2;;
    -p | --push )
       push="yes"; shift 2;;
    --ship-build )
       build=''; shift 2;;
    -k | --apikey )
       apikey="$2" shift 2;;
    -h | --help )
        usage; exit 1 ;;
    *)
        echo "Unknown option $1"
        usage; exit 2 ;;    
  esac
done

projects=(`cat ${workdir}/Components`)

function pack(){
   projectFolder="${srcPath}/${project}"
   cd ${projectFolder}
   rm -fr "$projectFolder/bin/Release"
   dotnet msbuild /p:Configuration=Release /p:SourceLinkCreate=true
   dotnet msbuild /t:pack /p:Configuration=Release /p:SourceLinkCreate=true
   projectMame=${projectMame:-}
   if [[ "${projectMame:-}" ]]; then
      projectPackPath="${projectFolder}/bin/Release/${projectMame}.*.nupkg"
   else
      projectPackPath="${projectFolder}/bin/Release/${project}.*.nupkg"
   fi
   mv $projectPackPath $workdir
  
}


if [[ $build ]]; then
  cd ${slnPath}
  dotnet restore Surging.sln
  for project in ${projects[@]}
  do
    pack --project ${project}
  done
  pack --project ${project} --projectMame "Surging.WebSocketCore"
  pack --project ${project} --projectMame "DotNetty.Codecs.DNS"
  cd ${workdir}
fi

if [[ $push ]]; then {
    if [[ !$apikey ]]; then {
        echo "未设置nuget仓库的APIKEY"
		exit 1
	}
  fi
	dotnet nuget push *.nupkg -s $nuget_repo -k $apikey
}
fi