import json
from confluent_kafka import Consumer, Producer, KafkaError
import os
import docker
from docker.errors import ContainerError
import shutil
import requests

# address where kafka hosted
KAFKA_SERVER = "localhost:9092"
WEB_SERVER = "localhost:4000"

# settings for producer and consumer
CONSUMER_SETTINGS = {
    'bootstrap.servers': KAFKA_SERVER,
    'group.id': 'mygroup',
    'client.id': 'client-1',
    'broker.address.family' : 'v4',
    'enable.auto.commit': True,
    'session.timeout.ms': 6000,
    'auto.offset.reset': 'earliest',
    'default.topic.config': {'auto.offset.reset': 'earliest'}
}

PRODUCER_SETTINGS = {'bootstrap.servers': KAFKA_SERVER, 'broker.address.family': 'v4'}

def delivery_report(err, msg):
    if err is not None:
        print('Message delivery failed: {}'.format(err))
    else:
        print('Message delivered to {} [{}]'.format(msg.topic(), msg.partition()))


def initiate_topic_and_executor(request):
    # initiate kafka consumer and producer
    producer = Producer(PRODUCER_SETTINGS)
    consumer = Consumer(CONSUMER_SETTINGS)

    # TODO: check for requried fields session_id and circuit_config

    # decide kafka topic for input and output
    input_topic = request['session_id'] + "_input"
    output_topic = request['session_id'] + "_output"
    consumer.subscribe([output_topic])

    # store topic information in a json file which will be forward into the docker container
    docker_topic_json = {'input_topic': input_topic, 'output_topic': output_topic}
    session_folder = '.' + request['session_id']
    os.mkdir(session_folder)
    with open(session_folder + "/topic.json", "w+") as f:
        json.dump(docker_topic_json, f, indent=4)

    # spawn docker container for the simulation

    os.chdir('../simulator-engine')
    docker_image = 'simulator'
    mounted_json_docker_path = '/json'
    mounted_json_host_path = os.path.dirname(os.path.realpath(__file__)) + '/../websocket/' + session_folder
    mounting_dictionary = {mounted_json_host_path: {'bind': mounted_json_docker_path}}

    try:
        docker_client = docker.from_env()
        container = docker_client.containers.run(
            docker_image,
            volumes=mounting_dictionary,
            network_mode='host',
            detach=True,
            remove=True
        )
    except ContainerError as e:
        error = str(e.stderr, 'utf-8')
        print(error)

def test_kakfa(request):


def kafka_handler(request):

    request = json.loads(request)

    # send circuit design to streaming executor

    for animal in request:

        device_list = request[animal]
        for device in device_list:

            params = {"id": device}
            circuit_design = None
            rs = requests.get(WEB_SERVER + "/circuit/getCircuitById?id={}".format(device))
            circuit_design = rs.text

            producer.produce(
                input_topic,
                key="design",
                value=json.dumps(circuit_design),
                callback=delivery_report)
            print("output sent to %s" % input_topic)

            # main loop for the simulation
            print("Collecting device sensor values")
            while True:
                # recieve inputs
                try:
                    sensor_inputs = await ws.receive_json()
                except Exception as e:
                    print("[Connection Terminated]")
                    break

                # TODO: check for fields in input
                print(sensor_inputs)

                # send inputs to kafka input topic
                producer.produce(input_topic, key="input", value=sensor_inputs, callback=delivery_report)
                print("output sent to %s" % input_topic)

                # listen to the kafka output topic for simulation result
                while True:
                    msg = consumer.poll(0.1)
                    if msg is None:
                        continue
                    elif not msg.error():
                        print('Received message: {0}'.format(msg.value()))
                        result = json.loads(str(msg.value().decode('ascii')))

                    elif msg.error().code() == KafkaError._PARTITION_EOF:
                        print('End of partition reached {0}/{1}'.format(msg.topic(), msg.partition()))
                    else:
                        print('Error occured: {0}'.format(msg.error().str()))

    shutil.rmtree(session_folder)
    container.stop()