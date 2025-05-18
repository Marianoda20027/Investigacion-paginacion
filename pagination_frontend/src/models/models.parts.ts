export interface PartsSearchFilters {
  category?: string;
  partCode?: string;
  technicalSpecs?: string;
  minStockQuantity?: number;
  maxStockQuantity?: number;
  minUnitWeight?: number;
  maxUnitWeight?: number;
  productionDateStart?: string;
  productionDateEnd?: string;
  lastModifiedStart?: string;
  lastModifiedEnd?: string;
}

export interface PartsSearchRequest {
  pageNumber?: number;
  pageSize?: number;
  filters?: PartsSearchFilters;
}

export interface Part {
  id: number;
  productionDate: string;
  lastModified: string;
  stockQuantity: number;
  unitWeight: number;
  partCode: string;
  category: string;
}

export interface PartsSearchResponse {
  total: number;
  pageNumber: number;
  pageSize: number;
  items: Part[];
}
