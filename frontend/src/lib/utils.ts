import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export type Success<T> = {
  data: T;
  error?: never;
};

export type Failure<E> = {
  data?: never;
  error: E;
};

export type Result<T, E = Error> = Success<T> | Failure<E>;

// Main wrapper function
export async function tryCatch<T, E = Error>(
  promise: Promise<T>
): Promise<Result<T, E>> {
  try {
    const data = await promise;
    return { data };
  } catch (error) {
    return { error: error as E };
  }
}

export function shouldNeverHappen(message: string): never {
  throw new Error(`Should never happen: ${message}`);
}
