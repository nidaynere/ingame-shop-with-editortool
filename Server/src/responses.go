package main

///responses
type ResponseApp struct {
	 Err int `json:"error"`
	 AppId string `json:"AppId"`
	 Template string `json:"Template"`
	 Templates []string `json:"Templates"`
}

type Result struct {
	Err int `json:"error"`
	Message string  `json:"message"`
}