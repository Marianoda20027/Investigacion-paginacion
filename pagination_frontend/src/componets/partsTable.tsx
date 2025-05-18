/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { Table, Input, InputNumber, DatePicker, Button, Space } from 'antd';
import { useEffect, useState } from 'react';
import { fetchParts } from '../services/service.parts';
import { Part } from '../models/models.parts';
import { Dayjs } from 'dayjs';
import { valueType } from 'antd/es/statistic/utils';
import dayjs from 'dayjs';

const { RangePicker } = DatePicker;

type NoUndefinedRangeValueType<T> = [T, T] | [];

export const PartsTable = () => {
  const [parts, setParts] = useState<Part[]>([]);
  const [countParts, setCountParts] = useState(20);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalRecords, setTotalRecords] = useState(0);
  const [responseTime, setResponseTime] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const [filters, setFilters] = useState({
    partCode: '',
    category: '',
    stockMin: null as valueType | null,
    stockMax: null as valueType | null,
    weightMin: null as valueType | null,
    weightMax: null as valueType | null,
    productionDate: [] as NoUndefinedRangeValueType<Dayjs>,
    lastModified: [] as NoUndefinedRangeValueType<Dayjs>,
    technicalSpecs: '',
  });

  const getParts = async (customPage?: number, customPageSize?: number) => {
    const startTime = performance.now(); // ⏱️ Inicio
    setIsLoading(true);

    const response = await fetchParts({
      filters: buildFilters(),
      pageNumber: customPage || pageNumber,
      pageSize: customPageSize || countParts,
    });
  
    const endTime = performance.now(); 
    setIsLoading(false);

    setResponseTime(endTime - startTime);
  
    const mappedParts = response.items.map((part: any) => ({
      id: part.id,
      partCode: part.partcode,
      category: part.category,
      stockQuantity: part.stockquantity,
      unitWeight: part.unitweight,
      productionDate: part.productiondate ? dayjs(part.productiondate).format('YYYY-MM-DD') : '',
      lastModified: part.lastmodified ? dayjs(part.lastmodified).format('YYYY-MM-DD HH:mm') : '',
      technicalSpecs: part.technicalspecs,
    }));
  
    setParts(mappedParts);
    setCountParts(response.pageSize);
    setTotalRecords(response.total);
    setPageNumber(response.pageNumber);
  };
  

  useEffect(() => {
    getParts(1, countParts); // al montar, inicia en página 1
  }, []);

  const buildFilters = () => {
    const builtFilters: Record<string, string> = {};

    if (filters.partCode) builtFilters.partCode = filters.partCode;
    if (filters.category) builtFilters.category = filters.category;
    if (filters.technicalSpecs) builtFilters.technicalSpecs = filters.technicalSpecs;

    if (filters.stockMin !== null && filters.stockMax !== null) {
      builtFilters.minStockQuantity = `${filters.stockMin}`;
      builtFilters.maxStockQuantity = `${filters.stockMax}`;
    }

    if (filters.weightMin !== null && filters.weightMax !== null) {
      builtFilters.minUnitWeight = `${filters.weightMin}`;
      builtFilters.maxUnitWeight = `${filters.weightMax}`;
    }

    if (filters.productionDate.length === 2) {
      builtFilters.productionDateStart = filters.productionDate[0].format('YYYY-MM-DD');
      builtFilters.productionDateEnd = filters.productionDate[1].format('YYYY-MM-DD');
    }

    if (filters.lastModified.length === 2) {
      builtFilters.lastModifiedStart = filters.lastModified[0].format('YYYY-MM-DDTHH:mm:ss');
      builtFilters.lastModifiedEnd = filters.lastModified[1].format('YYYY-MM-DDTHH:mm:ss');
    }

    return builtFilters;
  };

  const handleTableChange = (pagination: any) => {
    const newPage = pagination.current;
    const newPageSize = pagination.pageSize;
    setPageNumber(newPage);
    setCountParts(newPageSize);
    getParts(newPage, newPageSize);
  };

  const columns = [
    { title: 'ID', dataIndex: 'id', key: 'id' },
    { title: 'Part Code', dataIndex: 'partCode', key: 'partCode' },
    { title: 'Category', dataIndex: 'category', key: 'category' },
    { title: 'Stock Quantity', dataIndex: 'stockQuantity', key: 'stockQuantity' },
    { title: 'Unit Weight', dataIndex: 'unitWeight', key: 'unitWeight' },
    { title: 'Production Date', dataIndex: 'productionDate', key: 'productionDate' },
    { title: 'Last Modified', dataIndex: 'lastModified', key: 'lastModified' },
    { title: 'Technical Specs', dataIndex: 'technicalSpecs', key: 'technicalSpecs' },
  ];

  return (
    <div style={{ padding: 24 }}>
      <h2>Parts List</h2>

      <Space direction="vertical" size="middle" style={{ marginBottom: 24, width: '100%' }}>
      <div>
        <label>Page Size</label><br />
        <InputNumber
          min={1}
          value={countParts}
          onChange={(value) => {
            if (value) {
              setCountParts(value);
              setPageNumber(1); // Reinicia a la página 1
              getParts(1, value);
            }
          }}
        />
      </div>
        <Space wrap>
          <div>
            <label>Part Code</label><br />
            <Input onChange={e => setFilters({ ...filters, partCode: e.target.value })} />
          </div>
          <div>
            <label>Category</label><br />
            <Input onChange={e => setFilters({ ...filters, category: e.target.value })} />
          </div>
          <div>
            <label>Technical Specs</label><br />
            <Input onChange={e => setFilters({ ...filters, technicalSpecs: e.target.value })} />
          </div>
        </Space>

        <Space wrap>
          <div>
            <label>Stock Min</label><br />
            <InputNumber onChange={value => setFilters({ ...filters, stockMin: value })} />
          </div>
          <div>
            <label>Stock Max</label><br />
            <InputNumber onChange={value => setFilters({ ...filters, stockMax: value })} />
          </div>
          <div>
            <label>Weight Min</label><br />
            <InputNumber onChange={value => setFilters({ ...filters, weightMin: value })} />
          </div>
          <div>
            <label>Weight Max</label><br />
            <InputNumber onChange={value => setFilters({ ...filters, weightMax: value })} />
          </div>
        </Space>

        <Space wrap>
          <div>
            <label>Production Date</label><br />
            <RangePicker
              placeholder={['From', 'To']}
              onChange={dates => setFilters({ ...filters, productionDate: dates as NoUndefinedRangeValueType<Dayjs> })}
            />
          </div>
          <div>
            <label>Last Modified</label><br />
            <RangePicker
              showTime
              placeholder={['From', 'To']}
              onChange={dates => setFilters({ ...filters, lastModified: dates as NoUndefinedRangeValueType<Dayjs> })}
            />
          </div>
        </Space>

        <Button
  type="primary"
  loading={isLoading}
  onClick={() => {
    setFilters(filters);
    setPageNumber(1);
    getParts(1, countParts);
  }}
>
  Apply Filters
</Button>

      </Space>

      <div style={{ border: "1px solid lightgray", borderRadius: "33px", padding: "16px", marginBottom:"20px"}}>
        <h2>Additional Data</h2>
        <h5 style={{ color: "gray" }}>Current Page: {pageNumber}</h5>
        <h5 style={{ color: "gray" }}>Current Parts Per Page: {countParts}</h5>
        <h5 style={{ color: "gray" }}>Total Records: {totalRecords}</h5>
        <h5 style={{ color: "gray" }}>
          Backend Response Time: {responseTime !== null ? `${responseTime.toFixed(2)} ms` : 'N/A'}
        </h5>

      </div>
      <Space></Space>

      <Table
        dataSource={parts}
        columns={columns}
        rowKey="id"
        pagination={{
          current: pageNumber,
          pageSize: countParts,
          total: totalRecords,
          showSizeChanger: false,
        }}
        onChange={handleTableChange}
      />
    </div>
  );
};
