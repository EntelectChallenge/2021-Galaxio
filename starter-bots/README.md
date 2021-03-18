# Starter Bots

## Description
All the starters bots live in this repostitory.

## Bot versions
- Java = 11 
- NodeJS = 14
- .Net core = 3.9
- Python = 3.9
- Pytorch = 1.8.0
- Tensorflow = 2.4.1
- C++ = 9.3.0

## Accepted Bot languages for specific flows
# Automatic
For the automatic submission flow, we support the following languages.
- Java
- NodeJS
- .Net core
- Python
- Pytorch
- Tensorflow
- C++

# Manual
For the manual submission flow, we support the following languages.
- Java
- NodeJS
- .Net core
- Python
- Pytorch
- Tensorflow

## Repository Deployment Flow
This repository consist of 1 core branche, *master*
 - **master** = The staging branch will deploy to the **Staging** AWS account.  
 
Only the Reference Bot gets deploy to AWS ECR to the reference-bot registory repository.
The rest of the bots only gets build, to test that they do build.