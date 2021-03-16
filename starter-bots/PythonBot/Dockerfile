# Base image provided by Entelect Challenge
# Uncomment the FROM line the python base language you want to use. And uncomment the default FROM line.

# Default Python image.
# Base image is based on: python:3.9-alpine3.12
FROM public.ecr.aws/m5z5a5b2/languages/python:2021

# Pytorch Python image.
# Base image is based on: pytorch/pytorch:1.8.0-cuda11.1-cudnn8-runtime
# Pytorch version = 1.8.0
# NOTE: GPU cores are not available in Game environment
#FROM public.ecr.aws/m5z5a5b2/languages/python_pytorch:2021

# Tensorflow Python image.
# Base image is based on: tensorflow/tensorflow:2.4.1
# Tensorflow version = 2.4.1
# NOTE: GPU cores are not available in Game environment
#FROM public.ecr.aws/m5z5a5b2/languages/python_tensorflow:2021

WORKDIR /app

# Add your custom dependencies to the requirements.txt file to install them on build process.
COPY requirements.txt requirements.txt
RUN pip3 install -r requirements.txt

# The directory of the code to copy into this image, to be able to run the bot.
COPY . .

# The entrypoint to run the bot
ENTRYPOINT ["python3", "StarterBot.py"]