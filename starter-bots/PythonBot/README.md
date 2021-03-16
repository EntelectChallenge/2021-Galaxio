# Python Readme

## Python Installation:

	Simply download Python from https://www.python.org/downloads/ for the required OS
		- 3.8.8 for Python 3 Bots
	Alternatively you can also use a different distribution of python e.g. Anaconda.
		
	If you plan on using Anaconda, or any other python distribution,
	 ensure that the required paths have been added to your environment variables correctly.
	
	Once Installed ensure python has been installed correctly and works bt following these instructions :
		- In command line enter the command "python" (without quotes)
		- You should get a similar output to the following

			Python 3.8.8 (tags/v3.8.8:024d805, Feb 19 2021, 13:18:16) [MSC v.1928 64 bit (AMD64)] on win32
            Type "help", "copyright", "credits" or "license" for more information.
            >>>

**Note**: if you are using Anaconda you will still need to have a standalone installation of the Python 3.8. Anaconda does not expose the `python` or `py` commands required for the game-runner to start-up the python bots.

## Python Dependencies:

	Dependencies will be handled using PIP ( or conda if using Anaconda ), all dependencies should be supplied in a 
	requirements.txt file. 
	If you require any dependencies list them within the requirements.txt file and run
		
		pip install -r requirements.txt
		
		For Anaconda Users:
		    conda install --file requirements.txt
		

## Running the sample bot:

	Run the following:
		
		python PythonStarterBot.py

**Note**:
If any python commands were not included simply contact the Entelect Challenge team or create a Pull Request for the new command. 