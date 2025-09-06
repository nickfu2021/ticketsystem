FROM mcr.microsoft.com/dotnet/sdk:9.0

RUN apt-get update && apt-get install -y \
    curl \
    wget \
    git \
    vim \
    unzip \
    postgresql-client \
 && curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
 && apt-get install -y nodejs

WORKDIR /workspace

CMD ["sleep", "infinity"]