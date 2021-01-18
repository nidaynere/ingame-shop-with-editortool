package main

import (
	"log"
	"net/http"
	"github.com/gorilla/mux"
	"github.com/rs/cors"
	"os"
	"fmt"
	"encoding/json"
	"io/ioutil"
	"strconv"
	"io"
)

// Main Function
func main() {
	log.Printf("Payment API\n")

	checkAppsFolder ()
	
	// Router
	router := mux.NewRouter()

	// Services
	log.Printf("Services are loading...\n")

	router.HandleFunc("/SetApp/{appid}", SetApp).Methods("POST")
	router.HandleFunc("/SetApp/{appid}/{templateid}", SetTemplate).Methods("POST")
	router.HandleFunc("/GetApp/{appid}", GetApp).Methods("GET")
	router.HandleFunc("/GetApp/{appid}/{templateid}", GetTemplate).Methods("GET")
	router.HandleFunc("/RemoveTemplate/{appid}/{templateid}", RemoveTemplate).Methods("GET")

	handler := cors.AllowAll().Handler(router)

	log.Printf("Listening on 9001...\n")
	log.Fatal(http.ListenAndServe(":9001", handler))
}

// Utilities functions
func isError(err error) bool {
    if err != nil {
        fmt.Println(err.Error())
    }

    return (err != nil)
}

// Exists reports whether the named file or directory exists.
func checkAppsFolder () {
	var directory = "apps/"
	// create directory, if its not exist.
	if _, err := os.Stat(directory); os.IsNotExist(err) {
		log.Printf("[PAYMENTS] Apps folder doesnt exist. Creating ...\n")
		err := os.Mkdir(directory, 0755)
		
		if isError (err) {
			log.Printf("[PAYMENTS] Apps folder could not be created. Service will not work\n")
			return
		}
	}
}

func exists(name string) bool {
    if _, err := os.Stat(name); err != nil {
        if os.IsNotExist(err) {
            return false
        }
    }
    return true
}

func writeFile (path string, content string) bool {
	wrError := ioutil.WriteFile(path, []byte(content), 0644)

	if isError (wrError) {
		fmt.Println("[PAYMENTS] Writing failed %s", wrError.Error ())
		return false
	}

    return true
}

func createFile(path string) bool {
    // check if file exists
    var _, err = os.Stat(path)

    // create file if not exists
    if os.IsNotExist(err) {
        var file, err = os.Create(path)
        if isError(err) {
            return false
        }
        defer file.Close()
    }

	return true
}
//

func isAppCreated (appId string) bool {
	var directory = "apps/" + appId + "/"
	// check the app folder.
	if _, err := os.Stat(directory); os.IsNotExist(err) {
		return false
	}
	return true
} 

func SetApp (w http.ResponseWriter, r *http.Request) {
	// create response
	var response = Result {}

	params := mux.Vars(r)

	var appId = params["appid"]
	fmt.Println("[SetApp] appid %s", appId)

	appexist := isAppCreated (appId)

	if !appexist {
		err := os.Mkdir("apps/"+appId, 0755)
		
		if isError (err) {
			response.Err = 1
			response.Message = "[SetApp] Failure => Directory is not created."
			json.NewEncoder(w).Encode(response)
			fmt.Println(response.Message)
			return
		}

		// Create template folder.
		foldererr := os.Mkdir("apps/"+appId+"/templates", 0755)
			
		if isError (foldererr) {
			response.Err = 3
			response.Message = "[SetApp] Failure => Templates directory is not created."
			json.NewEncoder(w).Encode(response)
			fmt.Println(response.Message)
			return
		}
		//
	}

	var path string =  "apps/" + appId + "/app.json"

	body, bodyError := ioutil.ReadAll(r.Body)
	if isError (bodyError) {
		response.Err = 2
		response.Message = "[SetApp] Failure => Body could not be readed."
		fmt.Println(response.Message)
		return
	}

	bodyString := string(body)

	success := writeFile (path, bodyString)

	if (!success) {
		response.Err = 4
		response.Message = "[SetApp] Failure => Writing file is failed."
		fmt.Println(response.Message)
	} else {
		response.Err = 0
		response.Message = "[SetApp] Success."
		fmt.Println(response.Message)
	}

	json.NewEncoder(w).Encode(response)
}

func SetTemplate (w http.ResponseWriter, r *http.Request) {
	// create response
	var response = Result {}

	params := mux.Vars(r)

	var appId = params["appid"]
	var templateId = params["templateid"]
	fmt.Println("[SetTemplate] appid %s templateid %s", appId, templateId)

	appexist := isAppCreated (appId)

	if !appexist {
		response.Err = 1
		response.Message = "[SetTemplate] Failure => App is not exist."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	var directory = "apps/" + appId + "/templates/"

	// create directory, if its not exist.
	if _, err := os.Stat(directory); os.IsNotExist(err) {
		err := os.Mkdir(directory, 0755)
		
		if isError (err) {
			response.Err = 1
			response.Message = "[SetTemplate] Failure => Templates folder could not be created."
			json.NewEncoder(w).Encode(response)
			fmt.Println(response.Message)
			return
		}
	}

	var path = directory + templateId + ".json"

	body, bodyError := ioutil.ReadAll(r.Body)
	if isError (bodyError) {
		response.Err = 2
		response.Message = "[SetTemplate] Failure => Body could not be readed."
		fmt.Println(response.Message)
		return
	}
	bodyString := string(body)

	success := writeFile (path, bodyString)

	if (!success) {
		response.Err = 3
		response.Message = "[SetTemplate] Failure => Writing file is failed."
		fmt.Println(response.Message)
	} else {
		response.Err = 0
		response.Message = "[SetTemplate] Success."
		fmt.Println(response.Message)
	}

	json.NewEncoder(w).Encode(response)
}

func GetTemplate (w http.ResponseWriter, r *http.Request) {
	var response = Result {}

	// create response
	params := mux.Vars(r)

	var appId = params["appid"]
	var templateId = params["templateid"]

	fmt.Println("[GetTemplate] appid %s templateid %s", appId, templateId)

	appexist := isAppCreated (appId)

	if !appexist {
		response.Err = 1
		response.Message = "[GetTemplate] App is not found."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	var directory = "apps/" + appId + "/templates/"

	if _, err := os.Stat(directory); os.IsNotExist(err) {
		response.Err = 1
		response.Message = "[GetTemplate] Failure => Templates folder is not found."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	var path =	directory + "/" + templateId + ".json"
	if !exists (path) {
		response.Err = 2
		response.Message = "[GetTemplate] Failure => Template is not found on this app."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	Openfile, err := os.Open (path)

	defer Openfile.Close ()
	// download the 
	
	if (err != nil) {
		// file not found.
		fmt.Println ("file not found %s", path)	
	}

	// file found
	FileHeader := make ([]byte, 512)

	Openfile.Read (FileHeader)

	FileContentType := http.DetectContentType (FileHeader)

	FileStat, _ := Openfile.Stat ()
	FileSize := strconv.FormatInt (FileStat.Size (), 10)

	w.Header ().Set ("Content.Disposition", "attachment; filename=" + path)
	w.Header ().Set ("Content-Type", FileContentType)
	w.Header ().Set ("Content-Length", FileSize)

	// send the file here.
	Openfile.Seek (0,0)
	io.Copy (w, Openfile) // copy the file to client.

	return
}

func RemoveTemplate (w http.ResponseWriter, r *http.Request) {
	var response = Result {}

	// create response
	params := mux.Vars(r)

	var appId = params["appid"]
	var templateId = params["templateid"]

	fmt.Println("[GetTemplate] appid %s templateid %s", appId, templateId)

	appexist := isAppCreated (appId)

	if !appexist {
		response.Err = 1
		response.Message = "[GetTemplate] App is not found."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	var directory = "apps/" + appId + "/templates/"

	if _, err := os.Stat(directory); os.IsNotExist(err) {
		response.Err = 2
		response.Message = "[GetTemplate] Failure => Templates folder is not found."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	var path =	directory + "/" + templateId + ".json"
	if !exists (path) {
		response.Err = 3
		response.Message = "[GetTemplate] Failure => Template is not found on this app."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	// Remove
	e := os.Remove(path) 
    if e != nil { 
		response.Err = 4
		response.Message = "[RemoveTemplate] Failure => Template could not be deleted."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
	} 
	
	response.Err = 0
	response.Message = "[RemoveTemplate] Success."
	json.NewEncoder(w).Encode(response)
}

func GetApp (w http.ResponseWriter, r *http.Request) {
	var response = Result {}

	// create response
	params := mux.Vars(r)

	var appId = params["appid"]

	fmt.Println("[GetApp] appid %s", appId)

	appexist := isAppCreated (appId)

	if !appexist {
		response.Err = 1
		response.Message = "[GetTemplate] App is not found."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	var directory = "apps/" + appId + "/templates/"

	if _, err := os.Stat(directory); os.IsNotExist(err) {
		response.Err = 2
		response.Message = "[GetApp] Failure => Templates folder is not found."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	// Read app.json
	var appFile = "apps/" + appId + "/app.json";

	if !exists (appFile) {
		response.Err = 3
		response.Message = "[GetApp] Failure => Missing app file."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	file, _ := ioutil.ReadFile(appFile)
 
	data := ResponseApp {}
	
	data.AppId = appId
	err := json.Unmarshal([]byte(file), &data)

	data.Templates = data.Templates [:0]
 
	if isError (err) {
		response.Err = 4
		response.Message = "[GetApp] Failure => App json file could not be parsed. Probably broken."
		json.NewEncoder(w).Encode(response)
		fmt.Println(response.Message)
		return
	}

	// data.Templates should be list of the files in Templates folder.
	getfiles, err := ioutil.ReadDir(directory)
	for _, file := range getfiles {
		data.Templates = append(data.Templates, file.Name ())
    }

	json.NewEncoder(w).Encode(data)
}