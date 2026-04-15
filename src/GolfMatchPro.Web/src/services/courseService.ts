import api from './api';
import type { CourseDto, CreateCourseRequest } from '../types';

export const courseService = {
  getAll: () => api.get<CourseDto[]>('/courses').then(r => r.data),

  getById: (id: number) => api.get<CourseDto>(`/courses/${id}`).then(r => r.data),

  create: (request: CreateCourseRequest) =>
    api.post<CourseDto>('/courses', request).then(r => r.data),

  update: (id: number, request: CreateCourseRequest) =>
    api.put<CourseDto>(`/courses/${id}`, request).then(r => r.data),
};
