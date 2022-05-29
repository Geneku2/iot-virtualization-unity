FROM ubuntu:19.10

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    python3.7 \
    python3-pip \
    python3-setuptools \
    build-essential \
    libboost-all-dev \
    git  \
    nano && \
    pip3 install scipy

COPY ./SimulationPlatform /SimulationPlatform
COPY ./ArduinoSimulator /ArduinoSimulator
COPY ./kafka /kafka

RUN pip3 install -r kafka/requirements.txt && \
    cd /ArduinoSimulator && \
    make Utility/libArduino.so

ENTRYPOINT cp /json/topic.json /SimulationPlatform/topic.json && \
        cd /SimulationPlatform && \
        python3 -O streaming_executor.py