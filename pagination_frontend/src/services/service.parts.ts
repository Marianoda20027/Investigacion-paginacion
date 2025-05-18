import { doGet } from './http.service';
import {
  PartsSearchRequest,
  PartsSearchResponse
} from '../models/models.parts';

const API_URL = 'http://localhost:5000/api/parts';

const buildQueryParams = (params: PartsSearchRequest): string => {
  const query = new URLSearchParams();

  if (params.pageNumber) query.append('pageNumber', params.pageNumber.toString());
  if (params.pageSize) query.append('pageSize', params.pageSize.toString());

  console.log(params.filters)

  if (params.filters) {
    if (params.filters.category) query.append('category', params.filters.category);
    if (params.filters.partCode) query.append('partCode', params.filters.partCode);
    if (params.filters.technicalSpecs) query.append('technicalSpecs', params.filters.technicalSpecs);

    if (params.filters.minStockQuantity !== undefined) 
      query.append('minStockQuantity', params.filters.minStockQuantity.toString());
    if (params.filters.maxStockQuantity !== undefined) 
      query.append('maxStockQuantity', params.filters.maxStockQuantity.toString());

    if (params.filters.minUnitWeight !== undefined) 
      query.append('minUnitWeight', params.filters.minUnitWeight.toString());
    if (params.filters.maxUnitWeight !== undefined) 
      query.append('maxUnitWeight', params.filters.maxUnitWeight.toString());

    if (params.filters.productionDateStart) 
      query.append('productionDateStart', params.filters.productionDateStart);
    if (params.filters.productionDateEnd) 
      query.append('productionDateEnd', params.filters.productionDateEnd);

    if (params.filters.lastModifiedStart) 
      query.append('lastModifiedStart', params.filters.lastModifiedStart);
    if (params.filters.lastModifiedEnd) 
      query.append('lastModifiedEnd', params.filters.lastModifiedEnd);
  }

  return query.toString();
};

export const fetchParts = async (
  params: PartsSearchRequest = {} as PartsSearchRequest
): Promise<PartsSearchResponse> => {
  const queryString = buildQueryParams(params);
  const url = `${API_URL}?${queryString}`;

  console.log(url); // Verifica la URL generada
  const response = await doGet<PartsSearchResponse>(url);

  return response;
};
