# Contents

## Code 
A code is located at the root of repository. It's here:

### .\Tests.Common
Common classes used in tests projects.

### .\Tests.Unit
A set of isolated tests for CI process. These tests do not need any dependency.
 
### .\Tests.Integation
A set of tests to run over the existing infrastructure.
These tests communicate over Redis and stores a data to MySql. 
To run tests you need infrastructure components are up and connection strings are in configuration.yaml file.

### .\Tests.Auto
Black box tests. Those tests should be run over deployed NextServer instance. Also used for helm test.

### .\Core
Defines models and interfaces.

### .\NextServer
SignalR server implementation. 

### .\NextClient
Console NextServer client application.

### .\Services
Library with helper classes to solve use cases.

### .\SignalR
SignalR client implementation for NextClient and tests.

### .\Sql
Messages history repository implementation on top of MySql.

### .\Redis
Redis streams client; groups state persistence layer.

### .\ Configurator
Application to configure MySql database. Used from Helm hook after the chart deployed.

## CI
For CI purpose:
- a Helm folder contains a helm chart
- a build.ps1 file to build docker images locally
- git hub actions to perform builds on the CI stage

# Architecture and Approach

## Hi level picture
A solution is based on top of SignalR transport layer. Redis streams is used as messaging layer.
The data required to process messaging is also stored at the Redis side. 
Messages history is stored at the MySql database.

## Server
The server can be set up with multiple instances.
In fact, number of instances is number of replicas of docker container.
To leverage the number just specify an appropriate value in values.yaml of the Helm chart.
However, to makes things work you need to configure sticky sessions on your ingress traffic controller. 
Otherwise, the traffic may rich incorrect pod, which cause disconnections. 
Instructions how to (configure sticky sessions on ingress for Nginx ingress controller)[https://kubernetes.github.io/ingress-nginx/examples/affinity/cookie/].

## Client
There is a tiny client in the repository.
The client is console application with some help. So just test it. 

## Endpoint
The default endpoint is http://your-host:5000/next-chat. 
The host name is depend on your deployment configuration and for a local Kubernetes cluster is localhot.

## Inter cluster communications
Containers within Kubernetes clusters access each other by internal service name (hardcoded).

# TODO:
- Add telemetry.
- Add logging.
- Add resilence (circuit breaking, retries).
- Add unsubscribe from group when no user listens for on hub.
- Add input validation (Filed sizes, group membership, group existence, nulls).
- Check/add re-subscribe to joined groups on reconnect.
- Add PoD crash handling.
- Add friendly client error reporting.
- Make integration tests independent. At this moment tests require clear database and no parallelism.
- Handle multi-server user connection case. At this moment group membership not so fine when a user connected twice.
- Add readiness probe for Kubernetes containers.