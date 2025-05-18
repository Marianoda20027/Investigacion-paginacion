import axios, { AxiosInstance, AxiosResponse } from 'axios';

// API Real URL
const apiInstance: AxiosInstance = axios.create({
  baseURL: 'http://localhost:8080', // Cambiar al backend real cuando sea necesario
});


// ----------------------------------- MÉTODOS AXIOS -----------------------------------

// Método POST genérico
export const doPost = async <I, R>(payload: I, path: string): Promise<R> => {
  const response: AxiosResponse<R, I> = await apiInstance.post(path, payload);
  return response.data;
};

// Método PUT genérico
export const doPut = async <I, R>(payload: I, path: string): Promise<R> => {
  const response: AxiosResponse<R, I> = await apiInstance.put(path, payload);
  return response.data;
};

// Método GET genérico
export const doGet = async <R>(path: string): Promise<R> => {
  const response: AxiosResponse<R> = await apiInstance.get(path);
  return response.data;
};
