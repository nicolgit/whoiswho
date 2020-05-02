
  export interface suggester_response {
    "@odata.context": string;
    value: Value[];
  }
  
  export interface Value {
    "@search.text": string;
    Key: string;
  }

