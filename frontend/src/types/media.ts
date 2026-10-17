export interface MediaItem {
    userId: string;
    mediaId: string;
    title: string;
    description: string;
    originalFileName: string;
    s3ObjectKey: string;
    status: 'Pending' | 'Processing' | 'Ready' | 'Failed' | string;
    createdAt: string;
}

export interface CreateMediaRequest {
    title: string;
    description: string;
}

export interface CreateMediaResponse {
    message: string;
    mediaId: string;
}