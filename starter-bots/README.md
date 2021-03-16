# Starter Bots

## Description
All the starters bots live in this repostitory.

## Repository Deployment Flow
This repository consist of 1 core branche, *master*
 - **master** = The staging branch will deploy to the **Staging** AWS account.  
 
Only the Reference Bot gets deploy to AWS ECR to the reference-bot registory repository.
The rest of the bots only gets build, to test that they do build.