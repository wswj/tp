pipeline {

    // 全局环境变量
    environment {
        IMAGENAME     = 'tp'       // 镜像名称
        IMAGETAG      = '1.0.0'         // 镜像标签
        APPPORT       = '80'          // 应用占用的端口
        APPDIR        = '/app'      // 应用工作的目录
    }

    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:6.0' 
            args '-v /var/run/docker.sock:/var/run/docker.sock -v /usr/bin/docker:/usr/bin/docker'
        }
    }
    stages {

        
        // 开始构建，debug、test，在此过程中还原程序nuget依赖、输出 debug、单元测试等
        stage('Build') { 
            steps {
                sh 'dotnet restore'
            }
        }

        // 执行单元测试
        stage('Test') { 
            steps {
                sh 'dotnet test  --logger "console;verbosity=detailed"  --blame  --logger trx'
            }
        }
        
        // 正式发布
        stage('Publish') { 
            steps {
                sh 'dotnet publish --framework net6 --configuration Release --output dist'
            }
        }

        // 部署应用，
        // 这里选择将应用打包为 docker 镜像
        stage('Deploy') { 

            steps {
                sh  'touch Dockerfile'
                sh  'env'
                sh  'echo "start edit Dockerfile"'
                sh  'echo "FROM mcr.microsoft.com/dotnet/aspnet:6.0" >> Dockerfile'
                sh  'echo "COPY dist ${APPDIR}" >> Dockerfile'
                sh  'echo "EXPOSE ${APPPORT}" >> Dockerfile'
                sh  'echo "WORKDIR ${APPDIR}" >> Dockerfile'
                sh  'echo "VOLUME /app/wwwwroot" >> Dockerfile'
                sh  'echo "VOLUME /app/nlog" >> Dockerfile'
                sh  'echo \'ENTRYPOINT ["dotnet", "FluUrl.dll"]\' >> Dockerfile'

                sh 'cat Dockerfile'
                sh "docker build -t ${IMAGENAME}:${IMAGETAG} ."
            }
        }

        // 后续还可以执行 Docker 命令部署镜像，再使用健康检查等 API 检查容器是否正常，实现自动回退
        stage('RUN'){
        steps{
          sh 'docker run -d -p 443:443 --network backend -v /usrconfig/tp/data:/app/wwwroot -v /usrconfig/tp/log:/app/nlog --name  tpj --network backend tp'          
}  
      }
    }
}