FROM ubuntu:22.04

ENV DEBIAN_FRONTEND=noninteractive \
    DISPLAY=:99 \
    LANG=en_US.UTF-8

RUN apt-get update && apt-get install -y curl python3 python3-pip && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y \
    nodejs \
    xvfb \
    x11vnc \
    novnc \
    websockify \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

RUN npm install playwright@1.59.0
RUN npx playwright install-deps firefox

RUN pip3 install --no-cache-dir camoufox
RUN python3 -m camoufox fetch

COPY server.js start.sh ./
RUN chmod +x start.sh

EXPOSE 3000
EXPOSE 5900
EXPOSE 6080

CMD ["/app/start.sh"]
