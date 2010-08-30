#ifndef MATRIX_H
#define MATRIX_H

#include <memory>

/** A zero-based matrix of data. Used for automatic memory managment without the need to free
* the memoty inside the client function
*/
template <typename T>
class Matrix
{
private:
	int mRows;
	int mCols;
	T* data;
public:
	Matrix(int r, int c)
		: mRows(r)
		, mCols(c)
		, data(new T[mRows * mCols])
	{
	}

	Matrix(const Matrix& mtx)
		: mRows(mtx.mRows)
		, mCols(mtx.mCols)
		, data(new T[mRows * mCols])
	{
		std::memcpy(data, mtx.data, mRows * mCols);
	}

	int rows() const
	{
		return mRows;
	}

	int cols() const
	{
		return mCols;
	}

	T& operator()(int r, int c)
	{
		return data[r * mCols + c];
	}

	const T& operator()(int r, int c) const
	{
		return data[r * mCols + c];
	}

	~Matrix()
	{
		delete [] data;
	}
};

#endif